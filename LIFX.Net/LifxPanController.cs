using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using Windows.Networking;
using LIFX_Net.Messages;

namespace LIFX_Net
{
    public class LifxPanController
    {
        [XmlAttribute()]
        public string IPAddress { get; set; }

        [XmlAttribute()]
        public string MACAddress { get; set; }

        [XmlIgnore()]
        public Byte[] UID { get { return LifxHelper.StringToByteArray(MACAddress); } set { MACAddress = LifxHelper.ByteArrayToString(value); } }

        [XmlArray()]
        public List<LifxBulb> Bulbs { get; set; }

        public LifxPanController()
        {
            MACAddress = "";
            IPAddress = "";
            Bulbs = new List<LifxBulb>();
        }

        /// <summary>
        /// Uninitialized bulb, for detection for instance
        /// </summary>
        [XmlIgnore()]
        public static LifxPanController UninitializedPanController
        {
            get { return new LifxPanController() { IPAddress = LifxHelper.GetLocalBroadcastIPAddress(), MACAddress = "" }; }
        }
    }
}
