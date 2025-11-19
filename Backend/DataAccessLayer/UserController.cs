using System.Collections.Generic;
using System.Data.SQLite;


namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class UserController : DALController
    {
        private const string UserTable = "Users";
        public UserController() : base(UserTable) { }
        
        public override bool Insert(object dto)
        {
            var user = dto as UserDTO;
            using (var connection = new SQLiteConnection(_connectionString))
            {                
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO {_tableName} ({UserDTO.userEmailColumnName} ,{UserDTO.userPasswordColumnName}) " +
                    $"VALUES (@EmailVal,@PasswordVal);";

                    command.Parameters.Add(new SQLiteParameter("@EmailVal", user.Email));
                    command.Parameters.Add(new SQLiteParameter("@PasswordVal", user.Password));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public override bool Delete(object dto)
        {
            var user = dto as UserDTO;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"DELETE FROM {_tableName} WHERE Email = @Email;";
                    command.Parameters.Add(new SQLiteParameter("@Email", user.Email));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public bool Update(string email, string column, object value)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE {_tableName} SET [{column}] = @Value WHERE Email = @Email;";

                    command.Parameters.Add(new SQLiteParameter("@Email", email));
                    command.Parameters.Add(new SQLiteParameter("@Value", value));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public bool Exists(string email)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand())
                {
                    command.CommandText = $"SELECT * FROM {_tableName} WHERE Email = @Email LIMIT 1;";
                    command.Parameters.Add(new SQLiteParameter("@Email", email));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public bool DeleteAll()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand())
                {
                    command.CommandText = $"DELETE FROM {_tableName};";
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public List<UserDTO> SelectAll()
        {
            List<UserDTO> users = new List<UserDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand($"SELECT * FROM {_tableName};", connection);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(ConvertReaderToUser(reader));
                }
            }
            return users;
        }

        public UserDTO ConvertReaderToUser(SQLiteDataReader reader)
        {
            return new UserDTO(reader.GetString(0), reader.GetString(1));
        }

    }
}
