using System;

namespace LIFX_Net.Messages
{
    public class LifxGetPowerStateCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetPowerState;

        public LifxGetPowerStateCommand()
            : base(PACKET_TYPE, MessagePacketType.PowerState, true)
        {

        }
    }
}
