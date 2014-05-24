using System;

namespace LIFX_Net.Messages
{
    public class LifxSetPowerStateCommand : LifxCommand
    {
        private LifxPowerState mStateToSet = LifxPowerState.Off;
        private const CommandPacketType PACKET_TYPE = CommandPacketType.SetPowerState;

        public LifxSetPowerStateCommand(LifxPowerState stateToSet)
            : base(PACKET_TYPE, MessagePacketType.PowerState, true)
        {
            mStateToSet = stateToSet;
        }

        #region ILifxMessage Members

        public override byte[] GetRawMessage()
        {
            byte[] bytes = BitConverter.GetBytes((UInt16)mStateToSet);
            return bytes;
        }

        #endregion
    }
}
