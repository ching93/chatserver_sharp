using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace chatserver
{
    class Program
    {
        static byte[] obj;
        static int port = 8005; // порт для приема входящих запросов
        static void listenConnections(String msg)
        {
            
        }
        static void Main(string[] args)
        {
            //analPorn();
            List<int> l = new List<int>();
            var serverObj = new ChatServer();
            serverObj.Listen();

        }
        static void processConnection(IAsyncResult res)
        {

        }
        static void analPorn() {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),port);

            // создаем сервер
            Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listener.Bind(ipPoint);

                // начинаем прослушивание
                int socketCapacity = 10;
                listener.Listen(socketCapacity);

                /*Console.WriteLine("Сервер запущен. Ожидание подключений...");
                Socket[] sockets = new Socket[socketCapacity];
                Socket currentSocket = sockets[0];
                var cback = new AsyncCallback(processConnection);
                while (true)
                {
                    object state;
                    listener.BeginAccept(new AsyncCallback(processConnection), currentSocket);
                    foreach (Socket socket in sockets)
                    {
                        if (!socket.Blocking && socket!=currentSocket)
                            currentSocket = socket;
                    }
                }*/
                while (true)
                {
                    // получаем сообщение
                    Socket handler = listener.Accept();
                    Console.WriteLine("Получаем сообщение от "+ handler.RemoteEndPoint.ToString()); 
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());
                    
                    // отправляем ответ
                    //string message = "ваше сообщение доставлено";
                    //data = Encoding.Unicode.GetBytes(builder.ToString());
                    handler.Send(obj);

                    Console.WriteLine("Сообщение доставлено");
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}