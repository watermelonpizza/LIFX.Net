using System;

namespace LIFX_Net.Messages
{
    public class LifxGetTagsCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetTags;

        public LifxGetTagsCommand()
            : base(PACKET_TYPE, MessagePacketType.Tags, true)
        {
            
        }
    }
}
