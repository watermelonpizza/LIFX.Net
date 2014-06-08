using System;

namespace LIFX_Net.Messages
{
    public class LifxGetWiFiInfoCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetWifiInfo;

        public LifxGetWiFiInfoCommand()
            : base(PACKET_TYPE, MessagePacketType.WifiInfo, true)
        {

        }
    }
}