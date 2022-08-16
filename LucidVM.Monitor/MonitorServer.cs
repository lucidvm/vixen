using System;
using System.Collections.Generic;
using LucidVM.LEC;

namespace LucidVM.Monitor
{
    public class MonitorServer : LECServer
    {
        public event Action<IMachine> OnMachineAttach;
        public event Action<IMachine> OnMachineDetach;

        private static string[] MonitorServerCodebook = new string[]
        {
            "ping",
            "sync",
            "rect",
            "resize",
            "cursor",
            "mouse",
            "key",
            "tunnel",
            "cap",
            "auth",
            "list",
            "connect",
            "disconnect",
            "reset",
            "file"
        };

        Dictionary<string, IMachine> Machines = new Dictionary<string, IMachine>();

        public MonitorServer(int port = 2090, string host = "0.0.0.0") : base($"ws://{host}:{port}", MonitorServerCodebook)
        {
            OnEvent += (ctx, op, args) =>
            {
                switch (op)
                {
                    case "connect":
                        ctx.Channel = LECNormalize.EnsureString(args[1]);
                        if (Machines.ContainsKey(ctx.Channel))
                        {
                            IMachine machine = Machines[ctx.Channel];
                            VNCInfo info = machine.GetVNCInfo();
                            ctx.Send("connect", false);
                            string basevnc = $"vnc://{info.Address}:{info.Port}";
                            if (info.Password != null)
                            {
                                basevnc += $"#{info.Password}";
                            }
                            ctx.Send("cap", "reset", "file");
                            ctx.Send("tunnel", "vnc", basevnc);
                        }
                        else
                        {
                            ctx.Send("connect", true);
                        }
                        break;
                    case "reset":
                        if (Machines.ContainsKey(ctx.Channel))
                        {
                            Machines[ctx.Channel].Reset();
                            ctx.Send("reset", false);
                        }
                        else
                        {
                            ctx.Send("reset", true);
                        }
                        break;
                    case "file":
                        if (Machines.ContainsKey(ctx.Channel))
                        {
                            bool error = Machines[ctx.Channel].PushFile(
                                LECNormalize.EnsureString(args[1]),
                                LECNormalize.EnsureBuffer(args[2]),
                                LECNormalize.EnsureBoolean(args[3])
                            );
                            ctx.Send("file", error);
                        }
                        else
                        {
                            ctx.Send("file", true);
                        }
                        break;
                }
            };
        }

        public IMachine AddMachine(IMachine machine)
        {
            Machines[machine.GetEventChannel()] = machine;
            OnMachineAttach?.Invoke(machine);
            return machine;
        }

        public IMachine GetMachine(string channel)
        {
            return Machines[channel];
        }
    }
}
