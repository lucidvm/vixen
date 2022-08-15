using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LucidVM.Monitor;
using Vestris.VMWareLib;

namespace LucidVM.Monitor.VMware
{
    public enum VIXHostType
    {
        Workstation,
        ESXi
    }

    [XmlType("Host")]
    public class VIXHostInfo
    {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public VIXHostType Type = VIXHostType.Workstation;

        public string Address = null;
        public string Username = null;
        public string Password = null;
    }

    [XmlType("LoginInfo")]
    public class VIXGuestLoginInfo
    {
        [XmlAttribute]
        public string Username;
        [XmlAttribute]
        public string Password;
    }

    [XmlType("Machine")]
    public class VIXMachineInfo
    {
        [XmlAttribute]
        public string Channel;

        [XmlAttribute]
        public string Host;

        public string Path;

        public VNCInfo VNC = new VNCInfo
        {
            Address = "127.0.0.1",
            Port = 5900
        };

        public VIXGuestLoginInfo LoginInfo = new VIXGuestLoginInfo
        {
            Username = "Administrator",
            Password = ""
        };
    }

    public class VIXMachine : IMachine
    {
        private VIXHostInfo HostInfo;
        private VIXMachineInfo Info;

        private VMWareVirtualHost Host = new VMWareVirtualHost();
        private VMWareVirtualMachine VM;

        public VIXMachine(VIXHostInfo host, VIXMachineInfo info)
        {
            HostInfo = host;
            Info = info;
            switch (host.Type)
            {
                case VIXHostType.Workstation:
                    Host.ConnectToVMWareWorkstation();
                    break;
                // TODO: ESXi and friends
            }
            VM = Host.Open(info.Path);
        }

        public void Reset()
        {
            try
            {
                VM.Snapshots.GetCurrentSnapshot().RevertToSnapshot();
            }
            catch { }
            try
            {
                VM.PowerOn(0);
            }
            catch { }
        }

        public bool PushFile(string filename, byte[] data, bool run)
        {
            string tmp = Path.GetTempFileName();
            File.WriteAllBytes(tmp, data);

            VM.WaitForToolsInGuest();
            VM.LoginInGuest(Info.LoginInfo.Username, Info.LoginInfo.Password);
            string cmd = Path.Combine(VM.GuestEnvironmentVariables["windir"], "system32", "cmd.exe");
            string desktop = Path.Combine(VM.GuestEnvironmentVariables["userprofile"], "Desktop");
            string target = Path.Combine(desktop, filename);
            VM.CopyFileFromHostToGuest(tmp, target);
            if (run)
            {
                try
                {
                    // XXX: god why
                    VM.RunProgramInGuest(cmd, $"/c start \"\" \"{target}\"", 0);
                }
                catch { }
            }
            VM.LogoutFromGuest();

            File.Delete(tmp);

            return false;
        }

        public string GetEventChannel()
        {
            return Info.Channel;
        }

        public VNCInfo GetVNCInfo()
        {
            return Info.VNC;
        }

        public override string ToString()
        {
            return Info.Channel;
        }
    }
}
