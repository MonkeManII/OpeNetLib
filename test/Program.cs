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
            // Packet type 1: text message to server
            if (packet.Data[0] == 1)
            {
                if (packet.Server is not null)
                {
                    await packet.Server.Broadcast(packet.Data);
                } else if (packet.Client is not null)
                {
                    PacketDestructor packetReader = new(packet.Data);

                    Span<byte> bUsername = packetReader.ReadBytes();
                    Span<byte> bMessage = packetReader.ReadBytes();

                    string username = Encoding.UTF8.GetString(bUsername);
                    string message = Encoding.UTF8.GetString(bMessage);

                    Console.WriteLine("<{0}> {1}", username, message);
                }
            }
        }

        async static void SpawnClient(IPEndPoint ep)
        {
            Console.WriteLine("Attempting to connect to {0}!", ep);
            
            Client client = new(InterpretPacket);
            bool connect = await client.RequestConnect(ep, 1000);

            string username = PromptText("What's your nickname?");
            byte[] bUsername = Encoding.UTF8.GetBytes(username);

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

                byte[] message = Encoding.UTF8.GetBytes(text);
                PacketConstructor constructor = new(1, message.Length + bUsername.Length + 5);
                
                constructor.Write(bUsername);
                constructor.Write(message);

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
