using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Networking;
using Windows.Networking.Sockets;
using LIFX_Net.Messages;

namespace LIFX_Net
{
    public class LifxBulb
    {
        [XmlAttribute()]
        public string IPAddress { get; set; }

        [XmlAttribute()]
        public string MACAddress { get; set; }

        [XmlIgnore()]
        public Byte[] UID { get { return LifxHelper.StringToByteArray(MACAddress); } set { MACAddress = LifxHelper.ByteArrayToString(value); } }

        [XmlAttribute()]
        public string Label { get; set; }

        [XmlIgnore()]
        public LifxPanController PanController { get; set; }

        public LifxBulb() { }

        public LifxBulb(LifxPanController panController, string ipAddress, string macAddress)
        {
            PanController = panController;
            IPAddress = ipAddress;
            MACAddress = macAddress;
        }

        public async Task<LifxPowerStateMessage> GetPowerStateCommand()
        {
            LifxGetPowerStateCommand command = new LifxGetPowerStateCommand();
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxPowerStateMessage;
        }

        public async Task<LifxPowerStateMessage> SetPowerStateCommand(LifxPowerState stateToSet)
        {
            LifxSetPowerStateCommand command = new LifxSetPowerStateCommand(stateToSet);
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxPowerStateMessage;
        }

        public async Task<LifxPowerStateMessage> SetPowerStateCommand(string stateToSet)
        {
            LifxSetPowerStateCommand command = new LifxSetPowerStateCommand((LifxPowerState)Enum.Parse(typeof(LifxPowerState), stateToSet, true));
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxPowerStateMessage;
        }

        public async Task<LifxLabelMessage> GetLabelCommand()
        {
            LifxGetLabelCommand command = new LifxGetLabelCommand();
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxLabelMessage;
        }


        public async Task<LifxLabelMessage> SetLabelCommand(string newLabel)
        {
            LifxSetLabelCommand command = new LifxSetLabelCommand(newLabel);
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxLabelMessage;
        }

        public async Task<LifxLightStatusMessage> GetLightStatusCommand()
        {
            LifxGetLightStateCommand command = new LifxGetLightStateCommand();
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxLightStatusMessage;
        }


        public async Task SetColorCommand(LifxColour color, UInt32 fadeTime)
        {
            LifxSetLightStateCommand command = new LifxSetLightStateCommand(color.Hue, color.Saturation, color.Luminosity, color.Kelvin, fadeTime);
            await LifxCommunicator.Instance.SendCommand(command, this);
        }


        public async Task SetDimLevelCommand(UInt16 dimLevel, UInt32 fadeTime)
        {
            LifxSetDimAbsoluteCommand command = new LifxSetDimAbsoluteCommand(dimLevel, fadeTime);
            await LifxCommunicator.Instance.SendCommand(command, this);
        }

        public async Task<LifxTagsMessage> GetTagsCommand()
        {
            LifxGetTagsCommand command = new LifxGetTagsCommand();
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxTagsMessage;
        }

        public async Task<LifxTagLabelMessage> GetTagLabelsCommand(UInt64 tag)
        {
            LifxGetTagLabelsCommand command = new LifxGetTagLabelsCommand(tag);
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxTagLabelMessage;
        }

        public async Task<LifxTagsMessage> SetTagsCommand(UInt64 tags)
        {
            LifxSetTagsCommand command = new LifxSetTagsCommand(tags);
            return await LifxCommunicator.Instance.SendCommand(command, this) as LifxTagsMessage;
        }
    }
}
