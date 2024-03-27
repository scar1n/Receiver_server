using System.Net.Sockets;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Receiver_server
{
    internal class Program
    {
        
        static async Task Main(string[] args)
        {
            List<Socket> clients = new List<Socket>();
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 40432);
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipPoint);
            socket.Listen();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");
            //Task.Run(() => { CheckClientsAvalibale(clients); });

            while (true)
            {
                clients.Add(await socket.AcceptAsync());
                Console.WriteLine($"Адрес подключенного клиента: {clients.Last().RemoteEndPoint}");
                Task.Factory.StartNew(async () => await ProcessClientAsync(clients.Last()));
            }

        }

        private static void CheckClientsAvalibale(List<Socket> clients)
        {
            Thread.Sleep(1500);
            foreach (Socket client in clients)
            {
                if (!client.Connected) {
                    clients.Remove(client);
                    Console.WriteLine($"Отключен {client.RemoteEndPoint}");
                }
            }
        }

        private static async Task ProcessClientAsync(Socket client)
        {
            var buffer = new byte[1024];
            int recievedBytesCount = 0;
            recievedBytesCount = await client.ReceiveAsync(buffer);
            Console.Out.WriteLine($"" +
                $"imei str= {Encoding.UTF8.GetString(buffer.Skip(4).Take(15).ToArray())}");

            int remainingBytesCount = 0;
            while (true)
            {
                recievedBytesCount = await client.ReceiveAsync(buffer);
                if (recievedBytesCount != 0)
                {
                    for (int i = 0; i < recievedBytesCount; i += buffer[2 + i])
                    {
                        Console.Out.WriteLine($"" +
                        $"lon= {BitConverter.ToDouble(buffer.Skip(10).Take(4).ToArray())}" +
                        $"lat= {BitConverter.ToDouble(buffer.Skip(14).Take(4).ToArray())}"
                    );}
                }
            }
        }
    }
}