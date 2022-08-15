using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LucidVM.Monitor;
using LucidVM.Monitor.VMware;

namespace LucidVM.Vixen.CLI
{
    static class Program
    {
        static MonitorConfig Config;
        static MonitorServer Server;
        static Dictionary<string, VIXHostInfo> Hosts = new Dictionary<string, VIXHostInfo>();

        static void Main(string[] args)
        {
            try
            {
                string xmlpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vixen.xml");
                if (File.Exists(xmlpath))
                {
                    Config = XML.Read<MonitorConfig>(File.ReadAllText(xmlpath));
                }
                else
                {
                    Config = new MonitorConfig();
                    Config.Hosts.Add(new VIXHostInfo
                    {
                        Name = "local"
                    });
                    Config.Machines.Add(new VIXMachineInfo
                    {
                        Channel = "vm1",
                        Host = "local",
                        Path = @"C:\path\to\machine.vmx",
                        VNC = new VNCInfo
                        {
                            Address = "192.168.5.5",
                            Port = 5900,
                            Password = "hunter2"
                        }
                    });
                    File.WriteAllText(xmlpath, XML.Write(Config));
                    MessageBox.Show("Generated initial config. Please edit vixen.xml before continuing!", "vixen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Server = new MonitorServer(Config.Server.Port, Config.Server.Address);

                if (Config.Hosts.Count == 0)
                {
                    MessageBox.Show("No hosts defined!", "vixen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                foreach (VIXHostInfo info in Config.Hosts)
                {
                    Hosts[info.Name] = info;
                }

                if (Config.Machines.Count == 0)
                {
                    MessageBox.Show("No machines defined!", "vixen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                foreach (VIXMachineInfo info in Config.Machines)
                {
                    if (!Hosts.ContainsKey(info.Host))
                    {
                        throw new ArgumentException("unknown host " + info.Host);
                    }
                    VIXMachine machine = new VIXMachine(Hosts[info.Host], info);
                    Server.AddMachine(machine);
                    machine.Reset();
                }

                Server.Listen();
                Console.WriteLine("listening...");
                while (true) Console.Read();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Startup error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}