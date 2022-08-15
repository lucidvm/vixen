using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LucidVM.Monitor.VMware;

namespace LucidVM.Vixen
{
    [XmlType("Server")]
    public class MonitorServerInfo
    {
        [XmlAttribute]
        public string Address = "0.0.0.0";
        [XmlAttribute]
        public int Port = 2090;
    }

    [XmlType("Monitor")]
    public class MonitorConfig
    {
        public MonitorServerInfo Server = new MonitorServerInfo();
        public List<VIXHostInfo> Hosts = new List<VIXHostInfo>();
        public List<VIXMachineInfo> Machines = new List<VIXMachineInfo>();
    }
}
