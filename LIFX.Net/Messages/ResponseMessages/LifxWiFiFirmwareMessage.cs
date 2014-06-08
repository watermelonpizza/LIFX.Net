using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFX_Net.Messages
{
    public class LifxWiFiFirmwareMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.WifiFirmwareState;

        public LifxWiFiFirmwareMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public UInt64 Build
        {
            get
            {
                return BitConverter.ToUInt64(base.ReceivedData.Payload, 0);
            }
        }

        public UInt64 Install
        {
            get
            {
                return BitConverter.ToUInt64(base.ReceivedData.Payload, 8);
            }
        }

        public int Version
        {
            get
            {
                return BitConverter.ToInt32(base.ReceivedData.Payload, 16);
            }
        }
    }
}
