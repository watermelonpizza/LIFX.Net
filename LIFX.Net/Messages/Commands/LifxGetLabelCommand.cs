using System;

namespace LIFX_Net.Messages
{
    public class LifxGetLabelCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetLabel;

        public LifxGetLabelCommand()
            : base(PACKET_TYPE, MessagePacketType.Label, true)
        {
            
        }
    }
}
