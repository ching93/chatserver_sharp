using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.Common;
using ChatDbObjects;

namespace chatserver
{
    
    class DbUtils
    {
        string host = "127.0.0.1";
        int port = 3306;
        string database = "chat_db";
        string username = "root";
        string password = "1234";
        bool isConnected = false;
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
        public void setDb(String dbName)
        {
            this.database = dbName;
        }
        public int connect()
        {
            int code = 0;
            String connString = "Server=" + host + ";Database=" + database + ";port=" + port + ";User id=" + username + ";password=" + password;
            try
            {
                this.connection = new MySqlConnection(connString);
                connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("can't connect to db with "+e.Message);
                code = -1;
            }
            return code;
        }
        public DbDataReader querySelect(String sql)
        {
            if (!isConnected)
            {
                connect();
            }
            MySqlCommand command = new MySqlCommand(sql, this.connection);
            DbDataReader reader = command.ExecuteReader();
            return reader;
        }
        public int queryInsert(String sql)
        {
            MySqlCommand command = new MySqlCommand(sql, this.connection);
            int rowNum = command.ExecuteNonQuery();
            return rowNum;
        }
        public List<Entity> getAllUsers()
        {
            String sql = "SELECT login, Roles.id, Roles.name, firstname, lastname, email, registerTime FROM Users JOIN Roles ON Users.role_id=Roles.id;";
            List<Entity> users = new List<Entity>();
            DbDataReader reader = querySelect(sql);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    User cuser = new User();
                    //cuser.id = reader.GetInt16(0);
                    cuser.login = reader.GetString(0);
                    Role crole = new Role();
                    crole.id =  reader.GetInt16(1);
                    crole.Name = reader.GetString(2);
                    cuser.role = crole;
                    cuser.firstname = reader.GetString(3);
                    cuser.lastname = reader.GetString(4);
                    cuser.email = reader.GetString(5);
                    cuser.registerTime = reader.GetDateTime(6);
                    //byte[] buffer = new byte[256];
                    //Stream f = reader.GetStream(8);

                    users.Add(cuser);
                }
            }
            return users;
        }
        public void addUser(User newUser)
        {
            String sql = "INSERT INTO Users (login,firstname,lastname,email,role_id) VALUES (";
            sql += newUser.login + ",";
            sql += newUser.firstname + ",";
            sql += newUser.lastname + ",";
            sql += newUser.email + ",";
            sql += newUser.role.id + ");";

            if (queryInsert(sql) != 1)
                throw new Exception("cant' add into DB");
        }
        public bool checkUser(User user)
        {
            bool result = false;
            String sql = "SELECT password FROM Users WHERE login = '" + user.login + "';";
            var reader = querySelect(sql);
            if (reader.HasRows)
            {
                String password = reader.GetString(0);
                if (password == user.password)
                    result = true;
            }
            return result;
        }
    }
}
