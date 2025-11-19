using IntroSE.Kanban.Backend.DataAccessLayer.DTO;
using System.Collections.Generic;
using System.Data.SQLite;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class BoardController : DALController
    {
        private const string BoardTable = "Boards";
        public BoardController() : base(BoardTable) { }

        public override bool Insert(object dto)
        {
            var board = dto as BoardDTO;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO {_tableName} ({BoardDTO.boardIdColumn}, {BoardDTO.boardNameColumn}, {BoardDTO.boardOwnerEmailColumn}) " +
                                      $"VALUES (@Id, @Name, @OwnerEmail);";

                    command.Parameters.Add(new SQLiteParameter("@Id", board.Id));
                    command.Parameters.Add(new SQLiteParameter("@Name", board.Name));
                    command.Parameters.Add(new SQLiteParameter("@OwnerEmail", board.OwnerEmail));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public override bool Delete(object dto)
        {
            var board = dto as BoardDTO;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"DELETE FROM {_tableName} WHERE Id = @Id;";
                    command.Parameters.Add(new SQLiteParameter("@Id", board.Id));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public bool Update(long id, string column, object value)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE {_tableName} SET [{column}] = @Value WHERE Id = @Id;";
                    
                    command.Parameters.Add(new SQLiteParameter("@Value", value));
                    command.Parameters.Add(new SQLiteParameter("@Id", id));
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

        public List<BoardDTO> SelectAll()
        {
            List<BoardDTO> boards = new List<BoardDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand($"SELECT * FROM {_tableName};", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    boards.Add(ConvertReaderToBoard(reader));
                }
            }
            return boards;
        }
        public BoardDTO ConvertReaderToBoard(SQLiteDataReader reader)
        {
            return new BoardDTO(reader.GetInt64(0), reader.GetString(1), reader.GetString(2));
        }
    }
}
