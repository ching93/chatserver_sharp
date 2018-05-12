using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace chatserver
{
    [Serializable]
    abstract class Entity
    {
        public static byte[] Serialize(Entity obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            byte[] bytes = stream.ToArray();
            return bytes;
        }
        public static Entity Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return (Entity)formatter.Deserialize(stream);
        }
    }

    class Role : Entity
    {
        public int id;
        public String Name;
    }
    class User : Entity
    {
        public int id;
        public String login;
        public Role role;
        public String password;
        public String firstname;
        public String lastname;
        public String email;
        public Image photo;
    }
    class Exercise : Entity
    {
        public int id;
        public String name;
        public String[] Description;
        public DateTime allocTime;
        public DateTime duration;
        public bool IsDone;
        public User[] executers;
    }
    class ChatType : Entity
    {
        public int id;
        public String name;
    }
    class Message : Entity
    {
        public int id;
        public String[] text;
        public User author;
        public DateTime sendTime;
        public File[] embeddings;
    }
    class Chat : Entity
    {
        public int id;
        public String name;
        public ChatType type;
        public DateTime createTime;
        public Message[] messages;
        public User creator;
        public User[] members;
    }
    class File : Entity
    {
        public int id;
        public String path;
        public bool IsImage;
    }
}
