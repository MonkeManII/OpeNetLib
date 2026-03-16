using OpeNetLib.Packet;
using OpeNetLib;
using System.Net;
using System.Text;

namespace OpeNetLibTestApp
{
    public static class Program
    {
        static string PromptText(string question)
        {
            string? line = null;
            
            while (line is null)
            {
                Console.WriteLine(question);
                line = Console.ReadLine();

                if (line is null)
                {
                    Console.WriteLine("Answer must be non-null!");
                }
            }

            return line;
        }

        public static void Main()
        {
#if DEBUG
            Console.WriteLine("[DEBUG]");
#endif

            string choice = PromptText("'server' to host server; IP address to connect to server.\n\"localhost\" is permitted.");


            if (choice.Equals("server", StringComparison.CurrentCultureIgnoreCase))
            {
                SpawnServer();
            } else
            {
                choice = choice.Replace("localhost", "127.0.0.1");
                try
                {
                    IPEndPoint ep = IPEndPoint.Parse(choice);
                    SpawnClient(ep);
                } catch (FormatException)
                {
                    Console.WriteLine("{0} is not a valid IP!", choice);
                }
            }
        }

        static async void InterpretPacket(PacketCallbackParam packet)
        {
            // Packet type 0: handshake (hardcoded)

            // Packet type 1: text message (server relays to clients; client logs)
            if (packet.Data[0] == 1)
            {
                // If server recieved packet
                if (packet.Server is not null)
                {
                    await packet.Server.Broadcast(packet.Data);
                }
                
                // If client recieved packet
                else if (packet.Client is not null)
                {
                    PacketDestructor packetReader = new(packet.Data);

                    string username = packetReader.ReadUTF8();
                    string message = packetReader.ReadUTF8();

                    Console.WriteLine("<{0}> {1}", username, message);
                }
            }

            // Packet type 2: ping message
            if (packet.Data[0] == 2)
            {
                PacketConstructor newPacket = new(2, 300);

                // If server recieved packet
                if (packet.Server is not null)
                {
                    await packet.Server.Send(newPacket.ResultBytes(), 0);
                }
                
                // If client recieved packet
                else
                {
                    packet.Client?.Send([5]);
                }
            }
        }

        async static void SpawnClient(IPEndPoint ep)
        {
            Console.WriteLine("Attempting to connect to {0}!", ep);
            
            Client client = new(InterpretPacket);
            bool connect = await client.RequestConnect(ep, 1000);

            string username = PromptText("What's your nickname?");

            if (!connect)
            {
                Console.WriteLine("Connection timed out.");
                return;
            } else
            {
                Console.WriteLine("Connected to server socket at {0}!", client.ServerEndPoint);
            }

            while (true)
            {
                string? text = Console.ReadLine();
                if (text is null) continue;

                PacketConstructor constructor = new(1, 65536);
                
                constructor.WriteUTF8(username);
                constructor.WriteUTF8(text);

                client.Send(constructor.ResultBytes());
            }
        }

        static async void SpawnServer()
        {
            Server server = new(InterpretPacket);
            Console.WriteLine("Server started on port {0}!", server.Port);
        }
    }
}
