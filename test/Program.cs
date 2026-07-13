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
            PacketDestructor packetReader = new(packet.Data);
            byte type = packetReader.ReadByte();

            // Packet type 0: handshake (hardcoded)

            // Packet type 1: text message (server relays to clients; client logs)
            if (type == 1)
            {
                // If server recieved packet
                if (packet.Server is not null)
                {
                    await packet.Server.Broadcast(packet.Data);
                }

                string username = packetReader.ReadUTF8();
                string message = packetReader.ReadUTF8();

                Console.WriteLine("[{0}] >> {1}", username, message);
            }

            // Packet type 2: server message (server & client logs)
            // Server doesn't broadcast to prevent client from spoofing.
            if (type == 2)
            {
                string message = packetReader.ReadUTF8();
                Console.WriteLine("SERVER >> {0}", message);
            }

            // Packet type 3:
            //  CLIENT: kicked from server
            //  SERVER: client disconnected intentionally
            if (type == 3)
            {
                if (packet.Client is not null)
                {
                    Console.WriteLine("You have been forcefully disconnected.");
                    Console.WriteLine("Future attempts to send messages will not reach the server.");
                }
                
                if (packet.Server is not null)
                {
                    EndpointIdentifier id = packetReader.Read(EndpointIdentifier.Serializer);
                    IPEndPoint endpoint = id.ToEndpoint();

                    Console.WriteLine($"Client {endpoint} disconnected!");
                }
            }
        }

        async static void SpawnClient(IPEndPoint ep)
        {
            Console.WriteLine("Attempting to connect to {0}!", ep);
            
            Client client = new(InterpretPacket);
            bool connect = await client.RequestConnect(ep, 1000);

            if (!connect)
            {
                Console.WriteLine("Connection timed out.");
                return;
            } else
            {
                Console.WriteLine("Connected to server socket at {0}!", client.PermanentConnection?.RecieverEndpoint);
            }
            
            string username = PromptText("What's your nickname?");

            while (true)
            {
                string? text = Console.ReadLine();
                if (text is null) continue;

                PacketConstructor constructor = new(65536);

                constructor.WriteByte(1);
                constructor.WriteUTF8(username);
                constructor.WriteUTF8(text);

                client.Send(constructor.ResultBytes());
            }
        }

        static async void SpawnServer()
        {
            Server server = new(InterpretPacket);
            Console.WriteLine("Server started on port {0}!", server.NegotiatorPort);

            while (true)
            {
                string? text = Console.ReadLine();
                if (text is null) continue;

                PacketConstructor constructor = new(65536);

                constructor.WriteByte(2);
                constructor.WriteUTF8(text);

                await server.Broadcast(constructor.ResultBytes());
            }
        }
    }
}
