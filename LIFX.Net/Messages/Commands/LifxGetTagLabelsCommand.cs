using System;

namespace LIFX_Net.Messages
{
    public class LifxGetTagLabelsCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetTagLabels;
        private UInt64 mTags = 0;

        public LifxGetTagLabelsCommand(UInt64 tags)
            : base(PACKET_TYPE, MessagePacketType.TagLabels, false)
        {
            mTags = tags;
        }

        #region ILifxMessage Members

        public override byte[] GetRawMessage()
        {
            return BitConverter.GetBytes(mTags);
        }

        #endregion

    }
}
