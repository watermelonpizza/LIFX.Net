using System;

namespace LIFX_Net.Messages
{
    public class LifxSetDimAbsoluteCommand : LifxCommand
    {
        private UInt16 mDimLevel;
        private UInt32 mDurationMilliseconds;
        private const CommandPacketType PACKET_TYPE = CommandPacketType.SetDimAbsolute;

        public LifxSetDimAbsoluteCommand(UInt16 dimLevel, UInt32 durationMilliseconds)
            : base(PACKET_TYPE)
        {
            mDimLevel = dimLevel;
            mDurationMilliseconds = durationMilliseconds;
        }

        #region ILifxMessage Members

        public override byte[] GetRawMessage()
        {
            byte[] bytes = new byte[6];

            Array.Copy(BitConverter.GetBytes(mDimLevel), bytes, 2);
            Array.Copy(BitConverter.GetBytes(mDurationMilliseconds), 0, bytes, 2, 4);

            return bytes;
        }

        #endregion
    }
}
