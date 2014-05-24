using System;

namespace LIFX_Net.Messages
{
    public class LifxSetTagsCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.SetTags;
        private UInt64 mTagsValue = 0;

        public LifxSetTagsCommand(UInt64 tagsValue)
            : base(PACKET_TYPE)
        {
            mTagsValue = tagsValue;
        }

        public override byte[] GetRawMessage()
        {
            return BitConverter.GetBytes(mTagsValue);
        }


        public UInt64 TagsValue
        {
            get { return mTagsValue; }
            set { mTagsValue = value; }
        }
    }
}
