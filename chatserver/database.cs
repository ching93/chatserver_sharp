using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace chatserver
{
    class DbUtils
    {
        string host = "127.0.0.1";
        int port = 8000;
        string database = "simplehr";
        string username = "root";
        string password = "1234";
        MySqlConnection connection;
        DbDataReader reader;

        public DbUtils(String host,String user, String password)
        {
            this.host = host;
            this.username = user;
            this.password = password;
        }
        public DbUtils(String user, String password)
        {
            this.username = user;
            this.password = password;
        }
        public int connect()
        {
            int code = 0;
            String connString = "Server=" + host + ";Database=" + database + ";port=" + port + ";User id=" + username + ";password=" + password;
            try
            {
                this.connection = new MySqlConnection(connString);
            }
            catch (Exception e)
            {
                Console.WriteLine("can't connect to db");
                code = -1;
            }
            return code;
        }
        public void getAllUsers()
        {
            String sql = "SELECT id, login, Role.id, Role.name, firstname, lastname, photo FROM Users JOIN Role ON Users.role_id=Role.id;";
            List<Entity> users = new List<Entity>();
            MySqlCommand command = new MySqlCommand(sql, this.connection);
            DbDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    User cuser = new User();
                    cuser.id = reader.GetInt16(0);
                    cuser.login = reader.GetString(1);
                    Role crole = new Role();
                    crole.id =  reader.GetInt16(2);
                    crole.Name = reader.GetString(3);
                    cuser.role = crole;
                    cuser.firstname = reader.GetString(4);
                    cuser.lastname = reader.GetString(5);
                    byte[] buffer;
                }
            }
        }
    }
}
