using System;

namespace LIFX_Net.Messages
{
    /// <summary>
    /// Message sent to/from bulb, this is abstract class, inherited by the message implmentations
    /// </summary>
    public abstract class LifxCommand
    {
        public CommandPacketType CommandPacketType { get; private set; }
        public MessagePacketType ExpectedReturnMessagePacketType { get; private set; }
        public Boolean NeedReplyMessage { get; private set; }
        public Int32 WaitTimeBetweenRetry { get; set; }
        public DateTime TimeStamp { get; set; }
        public Boolean IsBroadcastCommand { get; set; }
        public UInt16 RetryCount { get; set; }
        public Boolean IsDiscoveryCommand { get; set; }

        public LifxCommand(CommandPacketType packetType, MessagePacketType returnMessagePacketType = MessagePacketType.None, Boolean needReply = false)
        {
            CommandPacketType = packetType;
            ExpectedReturnMessagePacketType = returnMessagePacketType;
            NeedReplyMessage = needReply;

            //Defaults
            WaitTimeBetweenRetry = 4000;
            RetryCount = 3;
            TimeStamp = DateTime.Now;
            IsBroadcastCommand = false;
            IsDiscoveryCommand = false;
        }

        public virtual byte[] GetRawMessage()
        { 
            return new byte[0];
        }

    }
}
