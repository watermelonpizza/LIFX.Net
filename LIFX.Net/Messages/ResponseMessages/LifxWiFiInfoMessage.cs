using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFX_Net.Messages
{
    public class LifxWiFiInfoMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.WifiInfo;

        public LifxWiFiInfoMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public float Signal
        {
            get 
            {
                return BitConverter.ToSingle(base.ReceivedData.Payload, 0);
            }
        }

        public int TX
        {
            get
            {
                return BitConverter.ToInt32(base.ReceivedData.Payload, 4);
            }
        }

        public int RX
        {
            get
            {
                return BitConverter.ToInt32(base.ReceivedData.Payload, 8);
            }
        }

        public short Mcu_Temperature
        {
            get
            {
                return
                    BitConverter.ToInt16(base.ReceivedData.Payload, 12);
            }
        }
    }
}
