using System;
using System.Collections.Generic;
using System.Text;
using LIFX_Net.Messages;

namespace LIFX_Net
{
    public static class LifxHelper
    {
        public const Int32 LIFX_PORT = 56700;
        public const String BROADCAST_IP_ADDRESS = "255.255.255.255";
        private const String LOCAL_PREFIX_IP = "169.254.";

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static LifxMessage PacketToMessage(LifxDataPacket packet)
        {
            switch (MessagePacketTypeToEnum(packet.PacketType))
            {
                case MessagePacketType.Label:
                    return new LifxLabelMessage(packet);
                case MessagePacketType.LightStatus:
                    return new LifxLightStatusMessage(packet);
                case MessagePacketType.PanGateway:
                    return new LifxPanGatewayStateMessage(packet);
                case MessagePacketType.PowerState:
                    return new LifxPowerStateMessage(packet);
                case MessagePacketType.TagLabels:
                    return new LifxTagLabelMessage(packet);
                case MessagePacketType.Tags:
                    return new LifxTagsMessage(packet);
                default:
                    return null;//throw new NotImplementedException();
            }
        }

        private static MessagePacketType MessagePacketTypeToEnum(uint packetType)
        {
            return (MessagePacketType)Enum.Parse(typeof(MessagePacketType), packetType.ToString());
        }

        public static string GetLocalBroadcastIPAddress()
        {
            string[] myipsplit = IdentifyMyIp().Split('.');
            string broadcastIP = myipsplit[0] + "." + myipsplit[1] + "." + myipsplit[2] + ".255";
            return broadcastIP;
        }

        public static string IdentifyMyIp()
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

    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double hue = 1.0;
        private double saturation = 1.0;
        private double luminosity = 1.0;

        private const double scale = 240.0;

        public double Hue
        {
            get { return hue * scale; }
            set { hue = CheckRange(value / scale); }
        }
        public double Saturation
        {
            get { return saturation * scale; }
            set { saturation = CheckRange(value / scale); }
        }
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public HSLColor(double hue, double saturation, double luminosity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        //public string ToRGBString()
        //{
        //    Color color = (Color)this;
        //    return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        //}

        //#region Casts to/from System.Drawing.Color
        //public static implicit operator Color(HSLColor hslColor)
        //{
        //    double r = 0, g = 0, b = 0;
        //    if (hslColor.luminosity != 0)
        //    {
        //        if (hslColor.saturation == 0)
        //            r = g = b = hslColor.luminosity;
        //        else
        //        {
        //            double temp2 = GetTemp2(hslColor);
        //            double temp1 = 2.0 * hslColor.luminosity - temp2;

        //            r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
        //            g = GetColorComponent(temp1, temp2, hslColor.hue);
        //            b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
        //        }
        //    }
        //    return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        //}

        //private static double GetColorComponent(double temp1, double temp2, double temp3)
        //{
        //    temp3 = MoveIntoRange(temp3);
        //    if (temp3 < 1.0 / 6.0)
        //        return temp1 + (temp2 - temp1) * 6.0 * temp3;
        //    else if (temp3 < 0.5)
        //        return temp2;
        //    else if (temp3 < 2.0 / 3.0)
        //        return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
        //    else
        //        return temp1;
        //}
        //private static double MoveIntoRange(double temp3)
        //{
        //    if (temp3 < 0.0)
        //        temp3 += 1.0;
        //    else if (temp3 > 1.0)
        //        temp3 -= 1.0;
        //    return temp3;
        //}
        //private static double GetTemp2(HSLColor hslColor)
        //{
        //    double temp2;
        //    if (hslColor.luminosity < 0.5)  //<=??
        //        temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
        //    else
        //        temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
        //    return temp2;
        //}

        //public static implicit operator HSLColor(Color color)
        //{
        //    HSLColor hslColor = new HSLColor();
        //    hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
        //    hslColor.luminosity = color.GetBrightness();
        //    hslColor.saturation = color.GetSaturation();
        //    return hslColor;
        //}
        //#endregion

        //public void SetRGB(int red, int green, int blue)
        //{
        //    HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
        //    this.hue = hslColor.hue;
        //    this.saturation = hslColor.saturation;
        //    this.luminosity = hslColor.luminosity;
        //}

        //public HSLColor() { }

        //public HSLColor(Color color)
        //{
        //    SetRGB(color.R, color.G, color.B);
        //}

        //public HSLColor(int red, int green, int blue)
        //{
        //    SetRGB(red, green, blue);
        //}

        //public static class Color
        //{
        //    public UInt16 R { get; set { if (value > 255) value = 255; else if (value < 0) value = 0; } }
        //    public UInt16 G { get; set { if (value > 255) value = 255; else if (value < 0) value = 0; } }
        //    public UInt16 B { get; set { if (value > 255) value = 255; else if (value < 0) value = 0; } }
            
        //    public Color() { }
        //}
    }
}




