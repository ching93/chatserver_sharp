using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
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
        public Entity[] getAllUsers(User currentUser)
        {
            User realUser = fillUserInfo(currentUser.login, currentUser.password);
            if (realUser.role.id != 1)
            {
                throw new Exception("У пользователя нет доступа");
            }
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
            reader.Close();
            return users.ToArray();
        }
        public User fillUserInfo(String login, String password)
        {
            if (!verifyUser(login, password))
            {
                throw new Exception("Нет разрешения у данного пользователя");
            }
            String sql = "SELECT login, Roles.id, Roles.name, firstname, lastname, email, registerTime FROM Users JOIN Roles ON Users.role_id=Roles.id WHERE login='";
            sql += login + "';";
            User user = new User();
            user.login = login;
            user.password = password;
            DbDataReader reader = querySelect(sql);
            if (reader.HasRows)
            {
                reader.Read();
                //cuser.id = reader.GetInt16(0);
                //user.login = reader.GetString(0);
                Role crole = new Role();
                crole.id = reader.GetInt16(1);
                crole.Name = reader.GetString(2);
                user.role = crole;
                user.firstname = reader.GetString(3);
                user.lastname = reader.GetString(4);
                user.email = reader.GetString(5);
                user.registerTime = reader.GetDateTime(6);
                //byte[] buffer = new byte[256];
                //Stream f = reader.GetStream(8);
            }
            return user;
        }

        public bool addUser(User currentUser,User newUser)
        {
            bool result = false;
            var realUser = fillUserInfo(currentUser.login, currentUser.password);
            if (realUser.role.id == 1)
            {
                String sql = "SELECT id, name from Roles;";
                var reader = querySelect(sql);
                while (reader.Read())
                {
                    if (reader.GetString(1) == newUser.role.Name)
                    {
                        newUser.role.id = reader.GetInt16(0);
                        break;
                    }
                }
                reader.Close();
                Console.WriteLine("role id = " + newUser.role.id);

                sql = "INSERT INTO Users (login,password,firstname,lastname,email,role_id,registerTime) VALUES ('";
                sql += newUser.login + "', '";
                sql += newUser.password + "', '";
                sql += newUser.firstname + "', '";
                sql += newUser.lastname + "', '";
                sql += newUser.email + "', '";
                sql += newUser.role.id + "', '";
                var now = DateTime.Now;
                sql += now.Year + "-" + now.Month + "-" + now.Day + " " + now.ToLongTimeString()+"');";
                if (queryInsert(sql) == 1)
                {
                    result = true;
                }
            }
            return result;
        }
        public bool verifyUser(String login, String password)
        {
            bool result = false;
            String sql = "SELECT password FROM Users WHERE login = '" + login + "';";
            var reader = querySelect(sql);
            if (reader.HasRows)
            {
                reader.Read();
                String pw = reader.GetString(0);
                if (pw == password)
                {
                    result = true;
                }
            }
            return result;
        }
        public Entity[] GetRoles()
        {
            List<Entity> result = new List<Entity>();
            String sql = "SELECT * from Roles;";
            DbDataReader reader = querySelect(sql);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Role crole = new Role();
                    crole.id = reader.GetInt16(0);
                    crole.Name = reader.GetString(1);
                    result.Add(crole);
                }
            }
            return result.ToArray();
        }
        public Entity[] GetChatTypes()
        {
            List<Entity> result = new List<Entity>();
            String sql = "SELECT * from ChatType;";
            DbDataReader reader = querySelect(sql);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ChatType ctype = new ChatType();
                    ctype.id = reader.GetInt16(0);
                    ctype.name = reader.GetString(1);
                    result.Add(ctype);
                }
            }
            return result.ToArray();
        }
    }
}
