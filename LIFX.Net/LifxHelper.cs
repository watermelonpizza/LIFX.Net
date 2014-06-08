using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Windows.UI;
using LIFX_Net.Messages;

namespace LIFX_Net
{
    public static class LifxHelper
    {
        public const Int32 LIFX_PORT = 56700;
        public const String BROADCAST_IP_ADDRESS = "255.255.255.255";
        private const String LOCAL_PREFIX_IP = "169.254."; // Stops the phone from latching onto local IP addresses, bulbs are never going to be on the local IP address range

        /// <summary>
        /// Converts a byte array to a string, used for converting a MAC address in the form of a hex byte array to a string
        /// </summary>
        /// <param name="ba">The MAC address to convert</param>
        /// <returns>String based MAC address</returns>
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        /// <summary>
        /// Converts a string to a byte array, used for converting a MAC address in the form of a hex based string to a byte array
        /// </summary>
        /// <param name="hex">The hex based string</param>
        /// <returns>Byte array of the MAC address</returns>
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Converts a raw <see cref="LifxDataPacket">LifxDataPacket</see> to a <see cref="LifxMessage">LifxMessage</see>
        /// </summary>
        /// <param name="packet">Raw DataPacket to convert from</param>
        /// <returns>A <see cref="LifxMessage">LifxMessage</see> if implemented or recognised otherwise null</returns>
        public static LifxMessage PacketToMessage(LifxDataPacket packet)
        {
            switch (MessagePacketTypeToEnum(packet.PacketType))
            {
                case MessagePacketType.Unknown:
                    break;
                case MessagePacketType.PanGateway:
                    return new LifxPanGatewayStateMessage(packet);
                case MessagePacketType.PowerState:
                    return new LifxPowerStateMessage(packet);
                case MessagePacketType.WifiInfo:
                    return new LifxWiFiInfoMessage(packet);
                case MessagePacketType.WifiFirmwareState:
                    return new LifxWiFiFirmwareMessage(packet);
                case MessagePacketType.WifiState:
                    break;
                case MessagePacketType.AccessPoint:
                    break;
                case MessagePacketType.Label:
                    return new LifxLabelMessage(packet);
                case MessagePacketType.Tags:
                    return new LifxTagsMessage(packet);
                case MessagePacketType.TagLabels:
                    return new LifxTagLabelMessage(packet);
                case MessagePacketType.LightStatus:
                    return new LifxLightStatusMessage(packet);
                case MessagePacketType.TimeState:
                    break;
                case MessagePacketType.ResetSwitchState:
                    break;
                case MessagePacketType.DummyLoad:
                    break;
                case MessagePacketType.MeshInfo:
                    break;
                case MessagePacketType.MeshFirmwareState:
                    break;
                case MessagePacketType.VersionState:
                    break;
                case MessagePacketType.Info:
                    break;
                case MessagePacketType.MCURailVoltage:
                    break;
                default:
                    return null;
            }

            return null;
        }
        
        /// <summary>
        /// Internal use only. Parses the raw message packettype in int form to the formatted Enum type
        /// </summary>
        /// <param name="packetType">The packet type</param>
        /// <returns>The enumerated type of <see cref="MessagePacketType">MessagePacketType</see></returns>
        private static MessagePacketType MessagePacketTypeToEnum(uint packetType)
        {
            return (MessagePacketType)Enum.Parse(typeof(MessagePacketType), packetType.ToString());
        }

        /// <summary>
        /// Gets the local /24 broadcast ip address (xxx.xxx.xxx.255)
        /// </summary>
        /// <returns>The broadcast IP as a string</returns>
        public static string GetLocalBroadcastIPAddress()
        {
            string[] myipsplit = IdentifyMyIp().Split('.');
            string broadcastIP = myipsplit[0] + "." + myipsplit[1] + "." + myipsplit[2] + ".255";
            return broadcastIP;
        }

        /// <summary>
        /// Internal use only. Gets the phones public IP address exclusing loopback and local IP addresses
        /// </summary>
        /// <returns>Returns the public IP address of the phone as a string</returns>
        private static string IdentifyMyIp()
        {
            List<string> MyIPAddresses = new List<string>();

            var MyHostnames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            foreach (var MyHostName in MyHostnames)
            {
                if (MyHostName.IPInformation != null)
                {
                    string MyIpAddress = MyHostName.DisplayName;
                    // Emulator: Ignore Link Local (169.254...)
                    if (MyIpAddress.StartsWith(LOCAL_PREFIX_IP))
                        continue;
                    // Emulator: ignore IPV6 addresses
                    if (MyIpAddress.Contains(":"))
                        continue;
                    MyIPAddresses.Add(MyIpAddress);
                }
            }

            // If more than one IP address remain, use the first one.
            // If WiFi is not connected, but the mobile carrier services is connected,
            // this may result in the phone reporting a carrier service IP address.
            // In a production app, you could filter out the carrier service IP address by accepting only 
            // addresses such as 192.xxx.xxx.xxx.
            return MyIPAddresses[0];
        }
    }

    /// <summary>
    /// The type of service either TCP or UDP the bulb is willing to work with. 
    /// Currently only UDP is implemented for this API
    /// </summary>
    public enum LifxPanServiceType : byte
    {
        UDP = 0x01,
        TCP = 0x02
    }

    public enum LifxPowerState : ushort
    {
        On = 1,
        Off = 0
    }

    public enum CommandPacketType : ushort
    {
        Unknown = 0x00,
        // Network
        GetPanGateway = 0x02,
        //Power
        GetPowerState = 0x14,
        SetPowerState = 0x15,
        //Wireless
        GetWifiInfo = 0x10,
        GetWifiFirmwareState = 0x12,
        GetWifiState = 0x12D,
        SetWifiState = 0x12E,
        GetAccessPoints = 0x130,
        SetAccessPoint = 0x131,
        //Labels and Tags
        GetLabel = 0x17,
        SetLabel = 0x18,
        GetTags = 0x1A,
        SetTags = 0x1B,
        GetTagLabels = 0x1D,
        SetTaglabels = 0x1E,
        //Brightness and Colors
        GetLightState = 0x65,
        SetLightState = 0x66,
        SetWaveform = 0x67,
        SetDimAbsolute = 0x68,
        SetDimRel = 0x69,
        //Time
        GetTime = 0x04,
        SetTime = 0x05,
        //Diagnostic
        GetResetSwitch = 0x07,
        GetDummyLoad = 0x09,
        SetDummyLoad = 0x0a,
        GetMeshInfo = 0x0C,
        GetMeshFirmware = 0x0E,
        GetVersion = 0x20,
        GetInfo = 0x22,
        GetMCURailVoltage = 0x24,
        Reboot = 0x26,
        SetFactoryTestMode = 0x27,
        DisableFactoryTestMode = 0x28
    }

    public enum MessagePacketType : ushort
    {
        Unknown = 0x00,
        None = 0x00,
        //Network
        PanGateway = 0x03,
        //Power
        PowerState = 0x16,
        //Wireless
        WifiInfo = 0x11,
        WifiFirmwareState = 0x13,
        WifiState = 0x12F,
        AccessPoint = 0x132,
        //Labels and Tags
        Label = 0x19,
        Tags = 0x1C,
        TagLabels = 0x1F,
        //Brightness and Colors
        LightStatus = 0x6B,
        //Time
        TimeState = 0x06,
        //Diagnostic
        ResetSwitchState = 0x08,
        DummyLoad = 0x0B,
        MeshInfo = 0x0D,
        MeshFirmwareState = 0x0E,
        VersionState = 0x21,
        Info = 0x23,
        MCURailVoltage = 0x25
    }

    public enum Waveform : byte
    {
        Saw = 0,
        Sine = 1,
        HalfSine = 2,
        Triangle = 3,
        Pulse = 4
    }

    #region Exception Classes

    public class BulbNotFoundException : Exception
    {
        public BulbNotFoundException() { }
        public BulbNotFoundException(string message) : base(message) { }
        public BulbNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class PanControllerNotFoundException : Exception
    {
        public PanControllerNotFoundException() { }
        public PanControllerNotFoundException(string message) : base(message) { }
        public PanControllerNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class ColorNotFoundException : Exception
    {
        public ColorNotFoundException() { }
        public ColorNotFoundException(string message) : base(message) { }
        public ColorNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    #endregion
}




