using System;

namespace LIFX_Net.Messages
{
    public class LifxTagsMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.Tags;

        public LifxTagsMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public UInt64 Tags
        {
            get 
            {
                return BitConverter.ToUInt64(base.ReceivedData.Payload, 0);
            }
        }
    }
}
