using System;

namespace LIFX_Net.Messages
{
    public class LifxGetLightStateCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetLightState;

        public LifxGetLightStateCommand()
            : base(PACKET_TYPE, MessagePacketType.LightStatus, true)
        {

        }
    }
}
