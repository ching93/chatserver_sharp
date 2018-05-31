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
        int bufferSize = 512;
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
                    //Thread clientThread = new Thread(new ParameterizedThreadStart(Process));
                    var t = new Task(Process,client);
                    t.ContinueWith((Task) => Console.WriteLine("client is fucked off"));
                    t.Start();
                    //clientThread.Start(client);
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
            MemoryStream stream;
            Socket client = (Socket)arg;
            try {
                byte[] buffer = new byte[bufferSize];
                while (true)
                {
                    stream = new MemoryStream();
                    int totalBytes = 0;
                    Console.Write("took ");
                    do
                    {
                        short bytes = (short)client.Receive(buffer);
                        stream.Write(buffer, 0, bytes);
                        Console.Write(bytes+", ");
                        totalBytes += bytes;
                    }
                    while (client.Available > 0);
                    Console.WriteLine();
                    Console.WriteLine("Сообщение получено от " + client.RemoteEndPoint.ToString() + "в размере " + totalBytes);
                    dbRequest request = (dbRequest)DbObject.Deserialize(stream);
                    stream.Close();
                    dbResult result = ProcessDbObject(request);
                    SendDbResult(client, result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Разъединение " + client.RemoteEndPoint.ToString());
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }
        private void SendDbResult(Socket client, dbResult result)
        {
            byte[] bytes = DbObject.Serialize(result);
            MemoryStream stream = new MemoryStream(bytes);
            Console.WriteLine("need to send " + bytes.Length + " bytes");
            int totalBytes = 0;
            while (stream.Position<stream.Length)
            {
                byte[] buffer = new byte[bufferSize];
                int byteCount = stream.Read(buffer, 0, bufferSize);
                totalBytes += byteCount;
                client.Send(buffer,byteCount,SocketFlags.None);
            }
            Console.WriteLine("sent " + totalBytes + " bytes");
        }
        private dbResult ProcessDbObject(dbRequest request)
        {
            dbResult result = new dbResult(request.action, request.selectAction, false);
            try
            {
                switch (request.selectAction)
                {
                    case dbSelectAction.SELECT_ROLES:
                        result.objects = dbHandle.GetRoles();
                        break;
                    case dbSelectAction.SELECT_CHAT_TYPES:
                        result.objects = dbHandle.GetChatTypes();
                        break;
                    case dbSelectAction.SELECT_ALL_USERS:
                        result.objects = dbHandle.getAllUsers(request.currentUser);
                        break;
                }
                switch (request.action)
                {
                    case dbAction.ADD_USER:
                        User newUser = (User)request.entity;
                        if (dbHandle.addUser(request.currentUser,newUser))
                        {
                            result.objects = new Entity[] { newUser };
                        }
                        break;
                    case dbAction.VERIFY_USER:
                        User user = (User)request.entity;
                        user = dbHandle.fillUserInfo(user.login,user.password);
                        result.objects = new Entity[] { user };
                        result.isSuccessful = true;
                        break;
                }
                result.isSuccessful = true;
                Console.WriteLine("Отправлен результат "+result.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result.message = e.Message;
                result.objects = null;
            }
            finally
            {
                
            }
            return result;
        }
    }
}
