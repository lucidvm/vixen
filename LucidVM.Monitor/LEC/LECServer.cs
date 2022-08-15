using System;
using System.Collections.Generic;
using Fleck;
using MsgPack;

namespace LucidVM.LEC
{
    public class LECClientContext
    {
        public readonly LECServer Server;
        public readonly IWebSocketConnection WebSock;
        public string Channel;

        public LECClientContext(LECServer server, IWebSocketConnection ws)
        {
            Server = server;
            WebSock = ws;
        }

        public void Send(params MessagePackObject[] args)
        {
            try
            {
                WebSock.Send(Server.Pack(args));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"send to {WebSock.ConnectionInfo.ClientIpAddress} failed");
                Console.WriteLine(ex);
            }
        }
    }

    public class LECServer
    {
        public event Action<LECClientContext> OnClientAttach;
        public event Action<LECClientContext> OnClientDetach;
        public event Action<LECClientContext, string, MessagePackObject[]> OnEvent;

        private readonly string[] Codebook;
        private readonly Dictionary<string, int> InvCodebook = new Dictionary<string, int>();

        private readonly WebSocketServer Server;
        private readonly List<LECClientContext> Clients = new List<LECClientContext>();

        public LECServer(string path = "ws://0.0.0.0:1337", string[] codebook = null)
        {
            Server = new WebSocketServer(path);

            if (codebook == null)
            {
                codebook = new string[] { };
            }
            Codebook = codebook;
            for (int i = 0; i < Codebook.Length; i++)
            {
                InvCodebook[Codebook[i]] = i;
            }
        }

        public IEnumerable<LECClientContext> GetClients()
        {
            return Clients;
        }

        public void Listen()
        {
            Server.Start(socket =>
            {
                LECClientContext ctx = new LECClientContext(this, socket);
                Clients.Add(ctx);
                socket.OnClose += () =>
                {
                    Clients.Remove(ctx);
                    OnClientDetach?.Invoke(ctx);
                };
                socket.OnBinary += packed =>
                {
                    try
                    {
                        MessagePackObject[] args = LECSerdes.Unpack(packed);
                        OnEvent?.Invoke(ctx, LECNormalize.EnsureOpcode(args[0], Codebook), args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error while parsing incoming message");
                        Console.WriteLine(ex);
                    }
                };
                OnClientAttach?.Invoke(ctx);
            });
        }

        public byte[] Pack(params MessagePackObject[] args)
        {
            if (LECNormalize.Check<string>(args[0]))
            {
                // use brief opcode if in codebook
                string op = args[0].AsString();
                if (InvCodebook.ContainsKey(op))
                {
                    args[0] = InvCodebook[op];
                }
            }
            return LECSerdes.Pack(args);
        }

        public void Broadcast(params MessagePackObject[] args)
        {
            byte[] data = Pack(args);
            foreach (LECClientContext ctx in Clients)
            {
                try
                {
                    ctx.WebSock.Send(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"broadcast to {ctx.WebSock.ConnectionInfo.ClientIpAddress} failed");
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
