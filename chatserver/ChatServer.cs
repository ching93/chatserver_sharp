using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ChatDbObjects;
using System.Threading;
using System.IO;

namespace chatserver
{
    class ChatServer
    {
        int port = 8000;
        String IP = "127.0.0.1";
        IPEndPoint endPoint;
        public int socketCapacity = 10;
        Socket Server;
        List<Socket> clients;
        int bufferSize = 256;
        DbUtils dbHandle;

        private void Initialize()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<Socket>();
            dbHandle = new DbUtils("root", "1211");
            dbHandle.connect();
            Console.WriteLine("Запуск сервера");
            Server.Bind(endPoint);
        }
        public ChatServer()
        {
            Initialize();
        }
        public ChatServer(String IP, int port)
        {
            this.port = port;
            this.IP = IP;
            Initialize();
        }
        public void Listen()
        {
            try
            {
                Server.Listen(socketCapacity);
                while (true)
                {
                    Socket client = Server.Accept();
                    Console.WriteLine("Подключение " + client.RemoteEndPoint.ToString());
                    clients.Add(client);
                    Thread clientThread = new Thread(new ParameterizedThreadStart(Process));
                    clientThread.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                foreach (Socket client in clients)
                {
                    client.Close();
                }
                Server.Close();
            }

        }
        private void Process(object arg)
        {
            MemoryStream stream = new MemoryStream();
            Socket client = (Socket)arg;
            try {
                byte[] buffer = new byte[bufferSize];

                do
                {
                    int bytes = client.Receive(buffer);
                    stream.Write(buffer, 0, bytes);
                }
                while (client.Available > 0);
                Console.WriteLine("Сообщение получено от " + client.RemoteEndPoint.ToString());
                DbObject obj = DbObject.Deserialize(stream);
                obj = ProcessDbObject(obj);
                SendDbObject(client, obj);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Разъединение " + client.RemoteEndPoint.ToString());
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                stream.Close();
            }
        }
        private void SendDbObject(Socket client, DbObject obj)
        {
            byte[] bytes = DbObject.Serialize(obj);
            MemoryStream stream = new MemoryStream(bytes);
            byte[] buffer = new byte[bufferSize];
            while(stream.CanRead)
            {
                int byteCount = stream.Read(buffer, 0, bufferSize);
                client.Send(buffer);
            }
        }
        private DbObject ProcessDbObject(DbObject obj)
        {
            try
            {
                switch (obj.selectAction)
                {
                    case dbSelectAction.SELECT_ALL_USERS:
                        obj.objects = dbHandle.getAllUsers();
                        break;
                }
                switch (obj.action)
                {
                    case dbAction.ADD_USER:
                        User newUser = (User)obj.objects[0];
                        dbHandle.addUser(newUser);
                        break;
                }
                obj.action = dbAction.QUERY_SUCCESS;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                obj.action = dbAction.QUERY_ERROR;
                obj.objects = null;
            }
            finally
            {
                obj.selectAction = dbSelectAction.NONE;
            }
            return obj;
        }
    }
}
