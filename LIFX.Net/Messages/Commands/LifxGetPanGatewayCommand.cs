using System;

namespace LIFX_Net.Messages
{
    public class LifxGetPanGatewayCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetPanGateway;
        
        public LifxGetPanGatewayCommand()
            : base(PACKET_TYPE, MessagePacketType.PanGateway, true)
        {
            base.IsBroadcastCommand = true;
            base.IsDiscoveryCommand = true;
        }
    }
}
