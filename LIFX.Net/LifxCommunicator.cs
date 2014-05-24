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
        private static DatagramSocket lifxCommunicatorClient = new DatagramSocket();
        private static Dictionary<MessagePacketType, LifxMessage> commandsAwaitingResponse = new Dictionary<MessagePacketType, LifxMessage>(10);
        private static Dictionary<MessagePacketType, SemaphoreSlim> messageResumer = new Dictionary<MessagePacketType, SemaphoreSlim>();

        public bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

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

        private void lifxCommunicatorClient_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            if (IsDisposed)
                return;

            uint bufferArraySize = args.GetDataReader().UnconsumedBufferLength;
            Byte[] receiveBytes = new Byte[bufferArraySize];
            args.GetDataReader().ReadBytes(receiveBytes);

            string receiveString = LifxHelper.ByteArrayToString(receiveBytes);
            LifxDataPacket packet = new LifxDataPacket(receiveBytes);

            LifxMessage receivedMessage = LifxHelper.PacketToMessage(packet);

            if (receivedMessage != null)
            {
                if (receivedMessage.PacketType == MessagePacketType.PanGateway)
                {
                    lock (commandsAwaitingResponse)
                    {
                        if (commandsAwaitingResponse.ContainsKey(receivedMessage.PacketType))
                        {
                            commandsAwaitingResponse[receivedMessage.PacketType] = receivedMessage;
                            messageResumer[receivedMessage.PacketType].Release();
                        }
                    }

                    LifxPanController foundPanHandler = new LifxPanController()
                    {
                        MACAddress = LifxHelper.ByteArrayToString(receivedMessage.ReceivedData.PanControllerMac),
                        IPAddress = args.RemoteAddress.DisplayName
                    };
                    foundPanHandler.Bulbs.Add(new LifxBulb(foundPanHandler, args.RemoteAddress.DisplayName, LifxHelper.ByteArrayToString(receivedMessage.ReceivedData.PanControllerMac)));

                    PanControllerFound.Invoke(this, foundPanHandler);
                    return;
                }

                lock (commandsAwaitingResponse)
                {
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
        /// Discovers the PanControllers (including their bulbs)
        /// </summary>
        /// <returns>List of bulbs</returns>
        public async Task Discover()
        {
            LifxGetPanGatewayCommand getPanGatewayCommand = new LifxGetPanGatewayCommand();
            await SendCommand(getPanGatewayCommand, LifxPanController.UninitializedPanController);
        }

        public async Task<LifxMessage> SendCommand(LifxCommand command, LifxBulb bulb)
        {
            return await SendCommand(command, bulb.MACAddress, bulb.PanController.MACAddress, bulb.IPAddress);
        }

        public async Task<LifxMessage> SendCommand(LifxCommand command, LifxPanController panController)
        {
            return await SendCommand(command, "", panController.MACAddress, panController.IPAddress);
        }

        /// <summary>
        /// Sends command to a bulb
        /// </summary>
        /// <param name="command"></param>
        /// <param name="bulb">The bulb to send the command to.</param>
        /// <returns>Returns the response message. If the command does not trigger a response it will reurn null. </returns>
        public async Task<LifxMessage> SendCommand(LifxCommand command, string bulbMacAddress, string panControllerMacAddress, string remoteIPAddress)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The communicator needs to be initialized before sending a command.");

            //If the command requires a packet
            if (command.ExpectedReturnMessagePacketType != MessagePacketType.Unknown)
            {
                lock (commandsAwaitingResponse)
                {
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

                    //Send command then wait for reply. If mutex times out then subtract one from retry count and send again, if response achieved set retry to 0 and quit
                    if (returnedMessage == null && resumerSemaphore != null)
                    {
                        if (await SendCommandRaw(command, bulbMacAddress, panControllerMacAddress, remoteIPAddress))
                        {
                            if (!resumerSemaphore.Wait(command.WaitTimeBetweenRetry))
                                command.RetryCount--;
                        }

                        if (command.RetryCount == 0 && command.NeedReplyMessage)
                            throw new TimeoutException("No response heard from light in required time");
                    }
                    else
                    {
                        commandsAwaitingResponse.Remove(command.ExpectedReturnMessagePacketType);
                        messageResumer.Remove(command.ExpectedReturnMessagePacketType);

                        return returnedMessage;
                    }
                } while (command.RetryCount > 0);
            }
            else
            {
                await SendCommandRaw(command, bulbMacAddress, panControllerMacAddress, remoteIPAddress);

                return null;
            }

            return null;
        }

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

        //private void AddDiscoveredPanHandler(LifxPanController foundPanHandler)
        //{
        //    foreach (LifxPanController handler in foundPanControllers)
        //    {
        //        if (handler.MACAddress == foundPanHandler.MACAddress)
        //            return;//already added
        //    }

        //    foundPanHandler.Bulbs.Add(new LifxBulb(foundPanHandler, foundPanHandler.IPAddress, foundPanHandler.MACAddress));
        //    foundPanControllers.Add(foundPanHandler);

        //    if (PanControllerFound != null)
        //        PanControllerFound.Invoke(this, foundPanHandler);
        //}

        //private void AddDiscoveredBulb(string macAddress, string panController)
        //{
        //    foreach (LifxPanController controller in foundPanControllers)
        //    {
        //        if (controller.MACAddress == panController)
        //        {
        //            foreach (LifxBulb bulb in controller.Bulbs)
        //            {
        //                if (bulb.MACAddress == macAddress)
        //                    return;
        //            }

        //            controller.Bulbs.Add(new LifxBulb(controller, macAddress));
        //            return;
        //        }
        //    }

        //    throw new InvalidOperationException("Should not end up here basically.");
        //}

        //private Task<DatagramSocket> GetConnectedClient(LifxCommand command, HostName endPoint)
        //{
        //    if (mSendCommandClient == null)
        //    {
        //        return CreateClient(command, endPoint);
        //    }
        //    else
        //    { 
        //        if (command.IsBroadcastCommand)
        //        {
        //            if (mSendCommandClient.Information.RemoteAddress.DisplayName == BROADCAST_IP_ADDRESS) //TODO: MAY NOT BE DISPLAY NAME
        //            {
        //                return new Task<DatagramSocket>(() => { return mSendCommandClient; });
        //            }
        //            else
        //            {
        //                mSendCommandClient.Dispose();
        //                return CreateClient(command, endPoint);
        //            }
        //        }
        //        else
        //        {
        //            if (mSendCommandClient.Information.RemoteAddress.DisplayName == BROADCAST_IP_ADDRESS)
        //            {
        //                mSendCommandClient.Dispose();
        //                return CreateClient(command, endPoint); 

        //            }
        //            else
        //            {
        //                return new Task<DatagramSocket>(() => { return mSendCommandClient; });
        //            }
        //        }
        //    }
        //}
        //private async Task<DatagramSocket> CreateClient(LifxCommand command, HostName endPoint)
        //{
        //    if (command.IsBroadcastCommand)
        //    {
        //        mSendCommandClient = new DatagramSocket();

        //        await mSendCommandClient.ConnectAsync(new HostName(BROADCAST_IP_ADDRESS), LIFX_PORT.ToString());
        //        return mSendCommandClient;
        //    }
        //    else
        //    {
        //        mSendCommandClient = new DatagramSocket();

        //        await mSendCommandClient.ConnectAsync(endPoint, LIFX_PORT.ToString());
        //        return mSendCommandClient;
        //    }
        //}

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