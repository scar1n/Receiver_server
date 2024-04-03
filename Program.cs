using System.Net.Sockets;
using System.Net;
using System.Text;

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
                Task.Run(async () => await ProcessClientAsync(clients.Last()));
            }

        }

        private static void CheckClientsAvalibale(List<Socket> clients)
        {
            Thread.Sleep(1000);
            
            foreach (Socket client in clients)
            {
                if (!client.Connected) {
                    clients.Remove(client);
                    Console.WriteLine($"Отключен {client.RemoteEndPoint}");
                }
            }
            foreach (Socket client in clients) Console.Out.WriteLine(client.RemoteEndPoint);
        }

        private static async Task ProcessClientAsync(Socket client)
        {
            var buffer = new byte[100];
            List<byte> packet = new List<byte>();
            
            int recievedBytesCount = await client.ReceiveAsync(buffer);

            string IMEI = Encoding.UTF8.GetString(buffer.Skip(4).Take(15).ToArray());
            Console.Out.WriteLine(IMEI);

            while (true)
            {
                recievedBytesCount = await client.ReceiveAsync(buffer);
                packet.AddRange(buffer.ToList());

                if (recievedBytesCount != 0)
                {
                    int i = 0;
                    for (; i + packet[2 + i] < packet.Count; i += packet[2 + i])
                    {
                        ParseGeodata(packet.Take(packet[2 + i]).ToList());
                    }

                    for (int j = 0; j < i; j++)
                        packet.Remove(packet.First());
                }
                else
                {
                    await Console.Out.WriteLineAsync("Поток остановлен");
                    break;
                }
                Thread.Sleep(2000);
            }
        }
        private static void ParseGeodata(List<byte> geodata)
        {
            int mask = 0b00001111;

            byte[] latBytes = geodata.Skip(9).Take(4).ToArray();
            byte[] lonBytes = geodata.Skip(13).Take(4).ToArray();
            byte[] courseBytes = geodata.Skip(17).Take(2).ToArray();
            byte[] speedBytes = geodata.Skip(19).Take(2).ToArray();
            byte[] heightBytes = geodata.Skip(22).Take(2).ToArray();
            byte satCountBytes = geodata.Skip(25).Take(1).ToArray()[0];
            byte[] datetimeBytes = geodata.Skip(26).Take(4).ToArray();
            byte[] vBatteryBytes = geodata.Skip(32).Take(2).ToArray();

            double lat = BitConverter.ToSingle(latBytes);
            double lon = BitConverter.ToSingle(lonBytes);
            float course = BitConverter.ToUInt16(courseBytes) / 10;
            float speed = BitConverter.ToUInt16(speedBytes) / 10;
            int height = BitConverter.ToUInt16(heightBytes);
            int satCount = (satCountBytes & mask) + ((satCountBytes >> 4) & mask);
            uint seconds = BitConverter.ToUInt32(datetimeBytes);
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(seconds)
                .ToLocalTime();
            int vBattery = BitConverter.ToUInt16(vBatteryBytes);

            Console.WriteLine($"{lat}\t{lon}\t{course} {height} {speed} {satCount} {dateTime} {vBattery}");
        }
    }
}