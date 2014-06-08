using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using LIFX_Net.Messages;

namespace LIFX_Net
{
    public class LifxCommunicator : IDisposable
    {
        private static LifxCommunicator instance = new LifxCommunicator();
        private DatagramSocket lifxCommunicatorClient = new DatagramSocket();
        private Dictionary<MessagePacketType, LifxMessage> commandsAwaitingResponse = new Dictionary<MessagePacketType, LifxMessage>(10);
        private Dictionary<MessagePacketType, SemaphoreSlim> messageResumer = new Dictionary<MessagePacketType, SemaphoreSlim>();

        public bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Gets the Instance for the LifxCommunicator class
        /// </summary>
        public static LifxCommunicator Instance { get { return instance; } private set { instance = value; } }

        public event EventHandler<LifxMessage> MessageRecieved;
        public event EventHandler<LifxPanController> PanControllerFound;

        private LifxCommunicator()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the listner for bulb messages
        /// </summary>
        public async void Initialize()
        {
            lifxCommunicatorClient = new DatagramSocket();
            lifxCommunicatorClient.MessageReceived += lifxCommunicatorClient_MessageReceived;
            CoreApplication.Properties.Add("listener", lifxCommunicatorClient);
            await lifxCommunicatorClient.BindEndpointAsync(null, LifxHelper.LIFX_PORT.ToString());

            IsInitialized = true;
        }

        /// <summary>
        /// The listner clients message recieved event handler.
        /// This is where the message will be converted into a LifxMessage and will invoke the MessageRecieved event handler if anything is subscribed to it
        /// </summary>
        /// <param name="sender">The socket that recieved the packet</param>
        /// <param name="args">Where the packet information is contained</param>
        private void lifxCommunicatorClient_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            if (IsDisposed)
                return;

            uint bufferArraySize = args.GetDataReader().UnconsumedBufferLength;
            Byte[] receiveBytes = new Byte[bufferArraySize];
            args.GetDataReader().ReadBytes(receiveBytes);

            LifxDataPacket packet = new LifxDataPacket(receiveBytes);
            LifxMessage receivedMessage = LifxHelper.PacketToMessage(packet);

            if (receivedMessage != null) // Check to make sure the packet we recieved was sent from a LIFX bulb, others can be sending on the same port
            {
                // If the packet type is a pan gateway we need to handle this a bit differently
                if (receivedMessage.PacketType == MessagePacketType.PanGateway)
                {
                    // This locks the variable and lets any senders will waiting for a pan gateway know that we have received the message and you can stop retrying
                    lock (commandsAwaitingResponse)
                    {
                        // Ensure that there are senders awaiting a message
                        if (commandsAwaitingResponse.ContainsKey(receivedMessage.PacketType))
                        {
                            commandsAwaitingResponse[receivedMessage.PacketType] = receivedMessage; // Sets the reply (from bulb) message to this message
                            messageResumer[receivedMessage.PacketType].Release(); // Stops the senders from retrying to ask for pan gateway messages
                        }
                    }

                    // Sets up the pan handler class and assigns the bulb to it
                    LifxPanController foundPanHandler = new LifxPanController()
                    {
                        MACAddress = LifxHelper.ByteArrayToString(receivedMessage.ReceivedData.PanControllerMac),
                        IPAddress = args.RemoteAddress.DisplayName
                    };
                    foundPanHandler.Bulbs.Add(new LifxBulb(foundPanHandler, args.RemoteAddress.DisplayName, LifxHelper.ByteArrayToString(receivedMessage.ReceivedData.PanControllerMac)));

                    // Invoke the PanControllerFound event handler
                    PanControllerFound.Invoke(this, foundPanHandler);
                    return;
                }

                // If the incoming packet isn't a pan controller, just lock the variable 
                // and lets any senders that may be waiting for a return message know that we have received the message and you can stop retrying
                lock (commandsAwaitingResponse)
                {
                    // If there is someone waiting for a recieved message, i.e. the command is *meant* to receive something from the bulb
                    // let it know we have found it (Release) and assign the message to the varaible
                    // Otherwise we have received a LifxMessage we didn't ask for. Ensure that there is someone subscribed to the event handler and Invoke with the message
                    if (commandsAwaitingResponse.ContainsKey(receivedMessage.PacketType))
                    {
                        commandsAwaitingResponse[receivedMessage.PacketType] = receivedMessage;
                        messageResumer[receivedMessage.PacketType].Release();
                    }
                    else
                    {
                        if (MessageRecieved != null)
                            MessageRecieved.Invoke(this, receivedMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Tells the LifxCommunicator to start looking for bulbs, send out broadcast messages to the network awaiting a reply back
        /// Ensure you are subscribed to the PanControllerFound event handler as this function does not return anything, all bulb/pancontrollers found
        /// will be pushed to that event handler.
        /// </summary>
        public async Task Discover()
        {
            LifxGetPanGatewayCommand getPanGatewayCommand = new LifxGetPanGatewayCommand();
            await SendCommand(getPanGatewayCommand, LifxPanController.UninitializedPanController);
        }

        /// <summary>
        /// Tells the comminucator to send a command to the bulb specified
        /// </summary>
        /// <param name="command">The command to send to the bulb</param>
        /// <param name="bulb">The bulb to send the command to</param>
        /// <returns>Returns a message if the command is expecting one, otherwise will return null <see cref="LifxCommand.ExpectedReturnMessagePacketType"/></returns>
        public async Task<LifxMessage> SendCommand(LifxCommand command, LifxBulb bulb)
        {
            return await SendCommand(command, bulb.MACAddress, bulb.PanController.MACAddress, bulb.IPAddress);
        }

        /// <summary>
        /// Tells the comminucator to send a command to the pan controller specified
        /// </summary>
        /// <param name="command">The command to send to the pan controller</param>
        /// <param name="bulb">The pan controller to send the command to</param>
        /// <returns>Returns a message if the command is expecting one, otherwise will return null <see cref="LifxCommand.ExpectedReturnMessagePacketType"/></returns>
        public async Task<LifxMessage> SendCommand(LifxCommand command, LifxPanController panController)
        {
            return await SendCommand(command, "", panController.MACAddress, panController.IPAddress);
        }

        /// <summary>
        /// Sends a command to the specified IP address
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <param name="bulbMacAddress">The MAC address of the bulb that will receive the command (used for mesh networking)</param>
        /// <param name="panControllerMacAddress">The pan controller MAC address that will receive the command (used for mesh networking)</param>
        /// <param name="remoteIPAddress">The IP address that will physically receive the command first</param>
        /// <returns>Returns a message if the command is expecting one, otherwise will return null <see cref="LifxCommand.ExpectedReturnMessagePacketType"/></returns>
        private async Task<LifxMessage> SendCommand(LifxCommand command, string bulbMacAddress, string panControllerMacAddress, string remoteIPAddress)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The communicator needs to be initialized before sending a command.");

            // If the command requires a reply message handle it accordingly (TCP over UDP essentially) otherwise just send the command
            if (command.ExpectedReturnMessagePacketType != MessagePacketType.Unknown)
            {
                // This locks and sets up the variable
                lock (commandsAwaitingResponse)
                {
                    // If it contains a key already don't create a new one, use an old one
                    // TODO: This is to ensure for example if a user spams a GetLightState command, if we receive one LightStateMessage all the commands that are waiting will be dropped
                    // bar one, that returns the message
                    if (!commandsAwaitingResponse.ContainsKey(command.ExpectedReturnMessagePacketType))
                    {
                        commandsAwaitingResponse.Add(command.ExpectedReturnMessagePacketType, null);
                        messageResumer.Add(command.ExpectedReturnMessagePacketType, new SemaphoreSlim(0));
                    }
                }

                do
                {
                    LifxMessage returnedMessage = null;
                    SemaphoreSlim resumerSemaphore = null;
                    lock (commandsAwaitingResponse)
                    {
                        commandsAwaitingResponse.TryGetValue(command.ExpectedReturnMessagePacketType, out returnedMessage);
                        messageResumer.TryGetValue(command.ExpectedReturnMessagePacketType, out resumerSemaphore);
                    }

                    // Send command then wait for reply. If Semephore times out then subtract one from retry count and send again
                    // Otherwise if there is a response heard from the device remove the values from the varaibles and return the message received
                    if (returnedMessage == null)// && resumerSemaphore != null)
                    {
                        if (await SendCommandRaw(command, bulbMacAddress, panControllerMacAddress, remoteIPAddress))
                        {
                            // If a response has come in then the wait command will return true
                            if (!resumerSemaphore.Wait(command.WaitTimeBetweenRetry))
                                command.RetryCount--;
                        }

                        // If the retry count reaches 0 and the command is required to, or want a reply and none is heard from then throw an exception
                        if (command.RetryCount == 0 && command.NeedReplyMessage)
                            throw new TimeoutException("No response heard from light in required time");
                    }
                    else
                    {
                        commandsAwaitingResponse.Remove(command.ExpectedReturnMessagePacketType);
                        messageResumer.Remove(command.ExpectedReturnMessagePacketType);

                        return returnedMessage;
                    }
                } while (command.RetryCount > 0); // Continue to loop if no response is head and the reply count is not zero yet
            }
            else
            {
                await SendCommandRaw(command, bulbMacAddress, panControllerMacAddress, remoteIPAddress);

                return null;
            }

            return null;
        }

        /// <summary>
        /// Sends the command to the device specified
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <param name="bulbMacAddress">The MAC address of the bulb that will receive the command (used for mesh networking)</param>
        /// <param name="panControllerMacAddress">The pan controller MAC address that will receive the command (used for mesh networking)</param>
        /// <param name="remoteIPAddress">The IP address that will physically receive the command first</param>
        /// <returns>Returns false if the command couldn't be sent for whatever reason</returns>
        private async Task<bool> SendCommandRaw(LifxCommand command, string bulbMacAddress, string panControllerMacAddress, string remoteIPAddress)
        {
            try
            {
                LifxDataPacket packet = new LifxDataPacket(command);
                packet.TargetMac = LifxHelper.StringToByteArray(bulbMacAddress);
                packet.PanControllerMac = LifxHelper.StringToByteArray(panControllerMacAddress);

                using (var stream = await new DatagramSocket().GetOutputStreamAsync(new HostName(remoteIPAddress), LifxHelper.LIFX_PORT.ToString()))
                {
                    using (var writer = new DataWriter(stream))
                    {
                        writer.WriteBytes(packet.PacketData);
                        await writer.StoreAsync();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        #region IDisposable Members

        public void CloseConnections()
        {
            lifxCommunicatorClient.Dispose();
        }

        public void Dispose()
        {
            IsDisposed = true;
            CloseConnections();
        }

        #endregion
    }
}