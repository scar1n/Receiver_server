using System.Net.Sockets;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Data;
using System.Linq;

namespace Receiver_server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 40432);
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipPoint);
            socket.Listen();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");


            while (true)
            {
                using Socket client = await socket.AcceptAsync();
                Console.WriteLine($"Адрес подключенного клиента: {client.RemoteEndPoint}");
                await Task.Run(async () => await ProcessClientAsync(client));
            }

        }

        private static async Task ProcessClientAsync(Socket client)
        {
            var buffer = new byte[1024];
            int recievedBytesCount = 0;
            recievedBytesCount = await client.ReceiveAsync(buffer);
            await Console.Out.WriteLineAsync($"" +
                $"imei str= {Encoding.UTF8.GetString(buffer.Skip(4).Take(15).ToArray())}" +
                $"imei int8= {BitConverter.ToUInt16(buffer.Skip(4).Take(15).ToArray())}"
                );
            if (recievedBytesCount == 0)
            {
                client.DisconnectAsync(false);
            }
            while (true) 
            {
                recievedBytesCount = await client.ReceiveAsync(buffer);
                await Console.Out.WriteLineAsync();
                if (recievedBytesCount == 0)
                {
                    client.DisconnectAsync(false);
                }
            }
        }
    }
}