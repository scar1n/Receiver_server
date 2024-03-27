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
                using Socket client = socket.Accept();
                Console.WriteLine($"Адрес подключенного клиента: {client}");
                await Task.Factory.StartNew(() => ProcessClient(client));
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

        private static void ProcessClient(Socket client)
        {
            var buffer = new byte[1024];
            int recievedBytesCount = 0;
            recievedBytesCount = client.Receive(buffer);
            Console.Out.WriteLine($"" +
                $"imei str= {Encoding.UTF8.GetString(buffer.Skip(4).Take(15).ToArray())}" +
                $"imei int8= {BitConverter.ToUInt16(buffer.Skip(4).Take(15).ToArray())}"
                );
            if (recievedBytesCount == 0)
                client.DisconnectAsync(false);

            int remainingBytesCount = 0;
            while (true)
            {
                recievedBytesCount = client.Receive(buffer);
                if (recievedBytesCount == 0)
                {
                    client.DisconnectAsync(false);
                    break;
                }

                for (int i = 0; i < recievedBytesCount; i += buffer[2 + i])
                {
                    Console.Out.WriteLine($"" +
                    $"imei lon= {BitConverter.ToDouble(buffer.Skip(10).Take(4).ToArray())}" +
                    $"imei lat= {BitConverter.ToDouble(buffer.Skip(14).Take(4).ToArray())}"
                );
                }
;

            }
        }
    }
}