using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class BoardMembersController : DALController
    {
        private const string BoardMembersTable = "BoardMembers";

        public BoardMembersController() : base(BoardMembersTable) { }

        public List<string> GetBoardMembers(int boardId)
        {
            var members = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(null, connection))
                {
                    command.CommandText = $"SELECT {BoardMemberDTO.EmailColumn} FROM {BoardMembersTable} WHERE {BoardMemberDTO.BoardIdColumn}=@boardId;";
                    command.Parameters.AddWithValue("@boardId", boardId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            members.Add(reader.GetString(0));
                    }
                }
            }
            return members;
        }

        public override bool Insert(object dto)
        {
            var member = dto as BoardMemberDTO;
            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"INSERT INTO {BoardMembersTable} ({BoardMemberDTO.BoardIdColumn}, {BoardMemberDTO.EmailColumn}) VALUES (@boardId, @email);";
                command.Parameters.AddWithValue("@boardId", member.BoardID);
                command.Parameters.AddWithValue("@email", member.Email);
                return ExecNonQuery(command) > 0;
            }
        }

        public override bool Delete(object dto)
        {
            var member = dto as BoardMemberDTO;
            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"DELETE FROM {BoardMembersTable} WHERE {BoardMemberDTO.BoardIdColumn}=@boardId AND {BoardMemberDTO.EmailColumn}=@email;";
                command.Parameters.AddWithValue("@boardId", member.BoardID);
                command.Parameters.AddWithValue("@email", member.Email);
                return ExecNonQuery(command) > 0;
            }
        }

        public bool DeleteAll()
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"DELETE FROM {_tableName};";
                return ExecNonQuery(command) > 0;
            }
        }
    }
}