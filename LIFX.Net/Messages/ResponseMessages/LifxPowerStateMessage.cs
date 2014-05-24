using System;

namespace LIFX_Net.Messages
{
    public class LifxPowerStateMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.PowerState;

        public LifxPowerStateMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public LifxPowerState PowerState
        {
            get 
            {
                if (BitConverter.ToUInt16(ReceivedData.Payload, 0) == 0)
                    return LifxPowerState.Off;
                else
                    return LifxPowerState.On;
            }
        }

        public Boolean GetPowerStateAsBool()
        {
            if (PowerState == LifxPowerState.On)
                return true;
            else
                return false;
        }
    }
}
