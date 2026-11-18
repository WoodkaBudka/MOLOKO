using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ГЗЗЗ
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Users FindByLogin(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT ID, Role, Login, Password, IsBlocked, FailedAttempts " +
                        "FROM dbo.Users WHERE Login = @Login";
                    command.Parameters.AddWithValue("@Login", login);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        public bool LoginExists(string login, int excludeId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT COUNT(1) FROM dbo.Users WHERE Login = @Login AND ID <> @ID";
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@ID", excludeId);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public List<Users> GetAll()
        {
            var result = new List<Users>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT ID, Role, Login, Password, IsBlocked, FailedAttempts " +
                        "FROM dbo.Users ORDER BY ID";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(MapUser(reader));
                        }
                    }
                }
            }
            return result;
        }

        public int Add(Users user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO dbo.Users (Role, Login, Password, IsBlocked, FailedAttempts) " +
                        "VALUES (@Role, @Login, @Password, @IsBlocked, @FailedAttempts); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    AddCommonParameters(command, user);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public void Update(Users user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE dbo.Users SET Login = @Login, Password = @Password, Role = @Role, " +
                        "IsBlocked = @IsBlocked, FailedAttempts = @FailedAttempts WHERE ID = @ID";
                    AddCommonParameters(command, user);
                    command.Parameters.AddWithValue("@ID", user.ID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void IncrementFailedAttempts(int userId)
        {
            ExecuteSimple("UPDATE dbo.Users SET FailedAttempts = FailedAttempts + 1 WHERE ID = @ID", userId);
        }

        public void ResetFailedAttempts(int userId)
        {
            ExecuteSimple("UPDATE dbo.Users SET FailedAttempts = 0 WHERE ID = @ID", userId);
        }

        public void Block(int userId)
        {
            ExecuteSimple("UPDATE dbo.Users SET IsBlocked = 1 WHERE ID = @ID", userId);
        }

        public void Unblock(int userId)
        {
            ExecuteSimple("UPDATE dbo.Users SET IsBlocked = 0, FailedAttempts = 0 WHERE ID = @ID", userId);
        }

        private void ExecuteSimple(string sql, int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@ID", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void AddCommonParameters(SqlCommand command, Users user)
        {
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@Login", user.Login);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@IsBlocked", user.IsBlocked);
            command.Parameters.AddWithValue("@FailedAttempts", user.FailedAttempts);
        }

        private static Users MapUser(SqlDataReader reader)
        {
            return new Users
            {
                ID = reader.GetInt32(0),
                Role = reader.GetString(1),
                Login = reader.GetString(2),
                Password = reader.GetString(3),
                IsBlocked = reader.GetBoolean(4),
                FailedAttempts = reader.GetInt32(5)
            };
        }
    }
}