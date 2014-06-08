using System;

namespace LIFX_Net.Messages
{
    public class LifxGetWiFiFirmwareCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetWifiFirmwareState;

        public LifxGetWiFiFirmwareCommand()
            : base(PACKET_TYPE, MessagePacketType.WifiFirmwareState, true)
        {

        }
    }
}