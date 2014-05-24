using System;

namespace LIFX_Net.Messages
{
    public abstract class LifxMessage
    {
        private LifxDataPacket mData;
        private MessagePacketType mPacketType = MessagePacketType.Unknown;

        public LifxMessage(LifxDataPacket data, MessagePacketType packetType)
        {
            mData = data;
            mPacketType = packetType;
        }

        //public LifxMessage(MessagePacketType packetType)
        //{
        //    mPacketType = packetType;
        //}

        public LifxDataPacket ReceivedData
        {
            get { return mData; }
            set { mData = value; }
        }

        public MessagePacketType PacketType
        {
            get { return mPacketType; }
        }

    }
}
