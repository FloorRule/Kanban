using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class ColumnController : DALController
    {
        private const string ColumnTable = "Columns";

        public ColumnController() : base(ColumnTable) { }

        /// <summary>
        /// an a sql query to add a row to columns table
        /// </summary>
        /// <param name="dto"> column dto to add its fields to the table</param>
        /// <returns></returns>
        public override bool Insert(object dto)
        {
            var column = dto as ColumnDTO;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO {_tableName}({ColumnDTO.ColumnsBoardIdColumnName},{ColumnDTO.ColumnsOrdinalColumnName},{ColumnDTO.ColumnsLimitColumnName})"
                                        + $"VALUES(@Id,@Ordinal,@Limit)";

                    command.Parameters.Add(new SQLiteParameter("@Id", column.BoardID));
                    command.Parameters.Add(new SQLiteParameter("@Ordinal", column.ColumnOrdinal));
                    command.Parameters.Add(new SQLiteParameter("@Limit", column.TasksLimit));

                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }

        }

        /// <summary>
        /// an a sql query to delete board's columns from columns table
        /// </summary>
        /// <param name="boardID"> boardId to specify the row</param>
        /// <returns></returns>
        public override bool Delete(object dto)
        {
            var column = dto as ColumnDTO;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                   
                    command.CommandText = $"DELETE FROM {_tableName} WHERE BoardID = @Id";

                    SQLiteParameter Pram1 = new SQLiteParameter("@Id", column.BoardID);
                    command.Parameters.Add(Pram1);

                    command.Prepare();
                    return ExecNonQuery(command) > 0;
                }
                
            }


        }


        /// <summary>
        /// an a sql query to update in the columns table
        /// </summary>
        /// <param name="id"> boardId to specify the row</param>
        /// <param name="Ordinal"> wanted column ordinal</param>
        /// <param name="attributeName"> the column name in the table</param>
        /// <param name="attributeValue"> the new value to set in the tavle</param>>
        /// <returns></returns>
        public bool Update(int id, int Ordinal, string attributeName, int attributeValue)
        {

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE {ColumnTable} SET [{attributeName}] = @AttributeValue WHERE BoardID = @BoardId AND Ordinal = @ColOrdinal";

                    command.Parameters.Add(new SQLiteParameter(@"AttributeValue", attributeValue));
                    command.Parameters.Add(new SQLiteParameter(@"BoardId", id));
                    command.Parameters.Add(new SQLiteParameter(@"ColOrdinal", Ordinal));
                    command.Prepare();

                    return ExecNonQuery(command) > 0;
                }
            }
        }

        public List<ColumnDTO> SelectAll()
        {
            List<ColumnDTO> columns = new List<ColumnDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand($"SELECT * FROM {_tableName};", connection);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(ConvertReaderToColumn(reader));
                }
            }
            return columns;
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

        public ColumnDTO ConvertReaderToColumn(SQLiteDataReader reader)
        {
            ColumnDTO result = new ColumnDTO(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2));
            return result;
        }
    }
}
