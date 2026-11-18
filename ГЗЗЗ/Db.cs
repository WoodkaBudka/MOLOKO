using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace ГЗЗЗ
{
    public static class Db
    {
        private static readonly string ConnStr = "Data Source=.\\SQLEXPRESS;Initial Catalog=DairyFactory;Integrated Security=True;TrustServerCertificate=True";

        private static Dictionary<string, int> _fails = new Dictionary<string, int>();
        private static HashSet<string> _blocked = new HashSet<string>();

        public static Users GetUser(string login)
        {
            using (var c = new SqlConnection(ConnStr))
            {
                c.Open();
                var cmd = new SqlCommand("SELECT ID, Login, Password, Role FROM users WHERE Login=@l", c);
                cmd.Parameters.AddWithValue("@l", login);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        var u = new Users
                        {
                            ID = (int)r["ID"],
                            Login = r["Login"].ToString(),
                            Password = r["Password"].ToString(),
                            Role = r["Role"].ToString()
                        };
                        u.IsBlocked = _blocked.Contains(u.Login);
                        u.FailedAttempts = _fails.ContainsKey(u.Login) ? _fails[u.Login] : 0;
                        return u;
                    }
                }
            }
            return null;
        }

        public static void RegisterFail(string login)
        {
            if (!_fails.ContainsKey(login)) _fails[login] = 0;
            _fails[login]++;

            if (_fails[login] >= 3)
            {
                _blocked.Add(login);
            }
        }

        public static void ResetFail(string login)
        {
            if (_fails.ContainsKey(login)) _fails[login] = 0;
        }

        public static void Unblock(string login)
        {
            _blocked.Remove(login);
            if (_fails.ContainsKey(login)) _fails[login] = 0;
        }

        public static List<Users> GetAllUsers()
        {
            var list = new List<Users>();
            using (var c = new SqlConnection(ConnStr))
            {
                c.Open();
                using (var r = new SqlCommand("SELECT ID, Login, Password, Role FROM users ORDER BY ID", c).ExecuteReader())
                {
                    while (r.Read())
                    {
                        var u = new Users
                        {
                            ID = (int)r["ID"],
                            Login = r["Login"].ToString(),
                            Password = r["Password"].ToString(),
                            Role = r["Role"].ToString()
                        };
                        u.IsBlocked = _blocked.Contains(u.Login);
                        u.FailedAttempts = _fails.ContainsKey(u.Login) ? _fails[u.Login] : 0;
                        list.Add(u);
                    }
                }
            }
            return list;
        }

        public static bool LoginExists(string login, int excludeId)
        {
            using (var c = new SqlConnection(ConnStr))
            {
                c.Open();
                var cmd = new SqlCommand("SELECT COUNT(1) FROM users WHERE Login=@l AND ID<>@id", c);
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@id", excludeId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static void SaveUser(Users u)
        {
            using (var c = new SqlConnection(ConnStr))
            {
                c.Open();

                if (u.ID == 0) 
                {
                    var cmdMax = new SqlCommand("SELECT ISNULL(MAX(ID), 0) + 1 FROM Users", c);
                    int nextId = (int)cmdMax.ExecuteScalar();
                    string sql = "INSERT INTO Users (ID, Login, Password, Role) VALUES (@id, @l, @p, @r)";
                    var cmd = new SqlCommand(sql, c);
                    cmd.Parameters.AddWithValue("@id", nextId);
                    cmd.Parameters.AddWithValue("@l", u.Login);
                    cmd.Parameters.AddWithValue("@p", u.Password);
                    cmd.Parameters.AddWithValue("@r", u.Role);
                    cmd.ExecuteNonQuery();
                }
                else 
                {
                    string sql = "UPDATE Users SET Login=@l, Password=@p, Role=@r WHERE ID=@id";
                    var cmd = new SqlCommand(sql, c);
                    cmd.Parameters.AddWithValue("@l", u.Login);
                    cmd.Parameters.AddWithValue("@p", u.Password);
                    cmd.Parameters.AddWithValue("@r", u.Role);
                    cmd.Parameters.AddWithValue("@id", u.ID);
                    cmd.ExecuteNonQuery();
                }
            }
            if (!u.IsBlocked) Unblock(u.Login);
        }
    }
}