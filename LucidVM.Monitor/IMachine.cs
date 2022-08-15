using System;
using System.Xml;
using System.Xml.Serialization;

namespace LucidVM.Monitor
{
    [XmlType("VNC")]
    public class VNCInfo
    {
        [XmlAttribute]
        public string Address;
        [XmlAttribute]
        public int Port;
        [XmlAttribute]
        public string Password = null;
    }

    public interface IMachine
    {
        string GetEventChannel();
        VNCInfo GetVNCInfo();

        void Reset();
        bool PushFile(string filename, byte[] data, bool run);
    }
}
