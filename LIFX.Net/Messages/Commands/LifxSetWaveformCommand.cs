using System;

namespace LIFX_Net.Messages
{
    public class LifxSetWaveformCommand : LifxCommand
    {
        private Byte mStream = 0;
        private Byte mTransient = 0;
        private UInt16 mHue = 0;
        private UInt16 mSaturation = 0;
        private UInt16 mBrightness = 0;
        private UInt16 mKelvin = 0;
        private UInt32 mPeriod = 0;
        private float mCycles = 0;
        private UInt16 mDutyCycles = 0;
        private Waveform mWaveform = Waveform.Saw;  

        private const CommandPacketType PACKET_TYPE = CommandPacketType.SetWaveform;

        public LifxSetWaveformCommand(Byte stream, Byte transient, UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 period, float cycles, UInt16 dutycycles, Waveform waveform)
            : base(PACKET_TYPE)
        {
            mStream = stream;
            mTransient = transient;
            mHue = hue;
            mSaturation = saturation;
            mBrightness = brightness;
            mKelvin = kelvin;
            mPeriod = period;
            mCycles = cycles;
            mDutyCycles = dutycycles;
            mWaveform = waveform;
        }

        #region ILifxMessage Members

        public override byte[] GetRawMessage()
        {
            byte[] bytesToReturn = new byte[21];

            Array.Copy(BitConverter.GetBytes(mStream), 0, bytesToReturn, 0, 1);
            Array.Copy(BitConverter.GetBytes(mTransient), 0, bytesToReturn, 1, 1);
            Array.Copy(BitConverter.GetBytes(mHue), 0, bytesToReturn, 2, 2);
            Array.Copy(BitConverter.GetBytes(mSaturation), 0, bytesToReturn, 4, 2);
            Array.Copy(BitConverter.GetBytes(mBrightness), 0, bytesToReturn, 6, 2);
            Array.Copy(BitConverter.GetBytes(mKelvin), 0, bytesToReturn, 8, 2);
            Array.Copy(BitConverter.GetBytes(mPeriod), 0, bytesToReturn, 10, 4);
            Array.Copy(BitConverter.GetBytes(mCycles), 0, bytesToReturn, 14, 4);
            Array.Copy(BitConverter.GetBytes(mDutyCycles), 0, bytesToReturn, 18, 2);
            Array.Copy(BitConverter.GetBytes((Byte)mWaveform), 0, bytesToReturn, 20, 1);

            return bytesToReturn;
        }

        #endregion
    }
}
