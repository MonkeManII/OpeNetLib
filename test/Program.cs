using OpeNetLib.Packet;
using OpeNetLib;
using System.Net;
using System.Text;

namespace OpeNetLibTestApp
{
    public static class Program
    {
        static Server? server;
        static Client? client;

        static string? PromptText(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

        public static void Main()
        {
#if DEBUG
            Console.WriteLine("[DEBUG]");
#endif

            string? choice = PromptText("'server' to host server; IP address to connect to server.");

            if (choice is null)
            {
                Console.WriteLine("is null :(");
                return;
            }

            if (choice.Equals("server", StringComparison.CurrentCultureIgnoreCase))
            {
                SpawnServer();
            } else
            {
                IPEndPoint ep = IPEndPoint.Parse(choice.Replace("localhost", "127.0.0.1"));
                SpawnClient(ep);
            }

            while (true) ;
        }

        static void InterpretPacket(OriginPacket packet)
        {
            // Packet type 1: text message to server
            if (packet.Data[0] == 1)
            {
                if (client is null)
                {
                    // Server repeats message to all clients
                    byte[] originAddr = packet.Origin.Address.GetAddressBytes();
                    byte[] originPort = BitConverter.GetBytes(packet.Origin.Port);

                    byte[] newPacket = new byte[3 + originAddr.Length + originPort.Length + packet.Data.Length];

                    // packet type of 2 is relayed message.
                    newPacket[0] = 2;
                    
                    // byte  1: addr len
                    // bytes 2..(2 + [1]): addr
                    // byte  (3 + [1])..(7 + [1]): port
                    // bytes (7 + [1]).. : text

                    newPacket[1] = (byte)originAddr.Length;

                    originAddr.CopyTo(newPacket, 2);
                    originPort.CopyTo(newPacket, originAddr.Length + 3);
                    packet.Data.CopyTo(newPacket, originAddr.Length + 7);

                    server?.Broadcast(newPacket);
                }
            }

            // Packet type 2: relayed message from server
            if (packet.Data[0] == 2)
            {
                if (client is not null)
                {
                    // Client logs message
                    Span<byte> packetSpan = packet.Data.AsSpan();

                    byte addrLen = packetSpan[1];
                    Span<byte> addr = packetSpan[2..(addrLen + 2)];
                    Span<byte> port = packetSpan[(addrLen + 3)..(addrLen + 7)];

                    IPAddress originAddr = new(addr);
                    IPEndPoint origin = new(originAddr, BitConverter.ToInt32(port));
                    string message = Encoding.UTF8.GetString(packetSpan[(addrLen + 8)..]);

                    Console.WriteLine("<{0}> {1}", origin, message);
                }
            }
        }

        async static void SpawnClient(IPEndPoint ep)
        {
            Console.WriteLine($"Attempting to connect to {ep}!");
            
            client = new(InterpretPacket);
            bool connect = await client.RequestConnect(ep, 1000);

            if (!connect)
            {
                Console.WriteLine($"Connection timed out.");
                return;
            } else
            {
                Console.WriteLine($"Connected to server socket at {client.ServerEndPoint}!");
            }

            while (true)
            {
                string? text = Console.ReadLine();
                if (text is null) continue;

                byte[] message = Encoding.UTF8.GetBytes(text);
                byte[] packet = new byte[message.Length + 1];

                // Byte 0 determines packet type; type 1 is UTF8 text.
                packet[0] = 1;
                message.CopyTo(packet, 1);

                client.Send(packet);
            }
        }

        static async void SpawnServer()
        {
            server = new(InterpretPacket);
            Console.WriteLine($"Server started on port {server.Port}!");
        }
    }
}
