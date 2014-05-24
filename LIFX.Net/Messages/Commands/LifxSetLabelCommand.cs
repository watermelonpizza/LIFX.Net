using System;

namespace LIFX_Net.Messages
{
    public class LifxSetLabelCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.SetLabel;
        private string mLabelName = "";

       
        public LifxSetLabelCommand(string newLabelName)
            : base(PACKET_TYPE, MessagePacketType.Label, false)
        {
            mLabelName = newLabelName;    
        }

        public override byte[] GetRawMessage()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(mLabelName);

            byte[] payload = new byte[32];

            

            Array.Copy(bytes, payload, Math.Min(payload.Length,bytes.Length));

            return payload;
        }


        public string LabelName
        {
            get { return mLabelName; }
            set { mLabelName = value; }
        }
    }
}
