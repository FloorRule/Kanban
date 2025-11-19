using System;
using System.Data.SQLite;
using System.IO;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    abstract class DALController
    {
        protected readonly string _connectionString;
        protected readonly string _tableName;

        protected DALController(string tableName)
        {
            string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "kanban.db"));
            _connectionString = $"Data Source={path}; Version=3;";
            _tableName = tableName;
        }

        protected int ExecNonQuery(SQLiteCommand command)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"DAL Error executing non-query for table '{_tableName}'. Command: '{command.CommandText}'. Error: {e.Message}\nStackTrace: {e.StackTrace}");
                }
                finally
                {
                    command.Dispose();
                }
                return 0;
            }
        }

        protected SQLiteCommand CreateCommand(string sql, SQLiteConnection connection, params (string name, object value)[] parameters)
        {
            var command = new SQLiteCommand(sql, connection);
            foreach (var (name, value) in parameters)
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            return command;
        }


        public abstract bool Insert(object dto);
        public abstract bool Delete(object dto);
    }
}
