using OpeNetLib.Internals;
using OpeNetLib.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OpeNetLib
{
    public static class Program
    {
        static string? PromptText(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

        static int PromptInt(string question)
        {
            Console.WriteLine(question);
            int ret = 0;
            bool success = false;

            while (!success)
            {
                string? txt = PromptText("Please input an int.");
                try
                {
                    ret = Convert.ToInt32(txt);
                    success = true;
                } catch(FormatException)
                {
                    Console.WriteLine("That's not an int.");
                }
            }

            return ret;
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
                Server();
            } else
            {
                IPEndPoint ep = IPEndPoint.Parse(choice);
                Client(ep);
            }

            while (true) ;
        }

        async static void Client(IPEndPoint ep)
        {
            int retries = 60;
            int newPort = -1;
            UdpListenThread thread = new("Client Listen Thread");
            UdpTwoWay client = new((packet) =>
            {
                newPort = BitConverter.ToInt32(packet.Data);
            });
            thread.AddPoll(client);
            thread.StartThread();

            Console.WriteLine($"Attempting to connect to server {ep}! (reciever port: {client.RecieverPort})");
            await client.Send(BitConverter.GetBytes(client.RecieverPort), ep);

            Console.WriteLine($"Wating to be directed to port... ({retries} second timeout)");
            while (newPort == -1)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"timeout: {--retries}");

                if (retries <= 0)
                {
                    Console.WriteLine("Connection refused: timeout");
                    return;
                }
            }
            Console.WriteLine($"Directed to port {newPort}!");
            
            ep = IPEndPoint.Parse($"{ep.Address}:{newPort}");
            byte[] ping = Encoding.UTF8.GetBytes("PING!");

            while (true)
            {
                await client.Send(ping, ep);
                Console.WriteLine($"Pinging with {ping.Length} bytes of data.");
                Thread.Sleep(1000);
            }
        }

        static void Server()
        {
            Server server = new();
            Console.WriteLine($"Server started on port {server.Port}!");

            while (true) ;
        }
    }
}
