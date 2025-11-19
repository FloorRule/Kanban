using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class TaskController : DALController
    {
        private const string TaskTable = "Tasks";

        public TaskController() : base(TaskTable) { }

        /// <summary>
		/// an a sql query to add a task in the tasks table
		/// </summary>
		/// <param name="tsk"> task dto to add its fields to the tasks table</param>
		/// <returns></returns>
		public override bool Insert(object dto)
        {
            var tsk = dto as TaskDTO;
            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"INSERT INTO {_tableName} " +
                                  $"({TaskDTO.tasksBoardIDColumnName}, {TaskDTO.tasksOrdColumnName}, {TaskDTO.tasksIDColumnName}, " +
                                  $"{TaskDTO.tasksAssigneeColumnName}, {TaskDTO.tasksTitleColumnName}, {TaskDTO.tasksDescriptionColumnName}, " +
                                  $"{TaskDTO.tasksDueDateColumnName}, {TaskDTO.tasksCreationTimeColumnName}) " +
                                  $"VALUES (@BoardID, @Ordinal, @TaskID, @AssigneeEmail, @Title, @Description, @DueDate, @CreationTime);";

                command.Parameters.Add(new SQLiteParameter("@BoardID", tsk.BoardID));
                command.Parameters.Add(new SQLiteParameter("@Ordinal", tsk.ColumnOrdinal));
                command.Parameters.Add(new SQLiteParameter("@TaskID", tsk.TaskId));
                command.Parameters.Add(new SQLiteParameter("@AssigneeEmail", tsk.Assignee));
                command.Parameters.Add(new SQLiteParameter("@Title", tsk.Title));
                command.Parameters.Add(new SQLiteParameter("@Description", tsk.Description));
                command.Parameters.Add(new SQLiteParameter("@DueDate", tsk.DueDate.ToString("s")));
                command.Parameters.Add(new SQLiteParameter("@CreationTime", tsk.CreationTime.ToString("s")));
                command.Prepare();

                return ExecNonQuery(command) > 0;
            }
        }

        /// <summary>
		/// an a sql query to delete board's tasks from tasks table
		/// </summary>
		/// <param name="dto"> TaskDTO to specify the row</param>
		/// <returns></returns>
        public override bool Delete(object dto)
        {
            var task = dto as TaskDTO;

            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"DELETE FROM {_tableName} WHERE {TaskDTO.tasksBoardIDColumnName} = @BoardID AND {TaskDTO.tasksOrdColumnName} = @Ordinal AND {TaskDTO.tasksIDColumnName} = @TaskID;";
                command.Parameters.AddWithValue("@BoardID", task.BoardID);
                command.Parameters.AddWithValue("@Ordinal", task.ColumnOrdinal);
                command.Parameters.AddWithValue("@TaskID", task.TaskId);
                command.Prepare();

                return ExecNonQuery(command) > 0;
            }

        }

        /// <summary>
		/// an a sql query to update in the tasks table
		/// </summary>
		/// <param name="id"> taskid to specify the row</param>
		/// <param name="brdid"> task's boardid</param>
		/// <param name="attributeName"> the column name in the tasks table</param>
        /// <param name="attributeValue"> the new value to set in the table</param>>
		/// <returns></returns>
		public bool Update(int id, int brdid, string attributeName, string attributeValue)
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @value WHERE TaskId = @TaskID AND BoardID = @BoardID";
                command.Parameters.AddWithValue("@value", attributeValue);
                command.Parameters.AddWithValue("@TaskID", id);
                command.Parameters.AddWithValue("@BoardID", brdid);
                command.Prepare();

                return ExecNonQuery(command) > 0;
            }
        }

        /// <summary>
        /// Overload to allow any object type to be used as update value.
        /// </summary>
        public bool Update(int taskId, int boardId, string attributeName, object attributeValue)
        {
            return Update(taskId, boardId, attributeName, attributeValue.ToString());
        }

        public List<TaskDTO> SelectAll()
        {
            List<TaskDTO> tasks = new List<TaskDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand($"SELECT * FROM {_tableName};", connection);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(ConvertReaderToTask(reader));
                }
            }
            return tasks;
        }

        public bool DeleteAll()
        {
            using (var command = new SQLiteCommand())
            {
                command.CommandText = $"DELETE FROM {_tableName};";
                command.Prepare();

                return ExecNonQuery(command) > 0;
            }
        }

        public TaskDTO ConvertReaderToTask(SQLiteDataReader reader)
        {
            return new TaskDTO(
                reader.GetInt32(0),// BoardID
                reader.GetInt32(1),// Ordinal
                reader.GetInt32(2),// TaskID
                reader.GetString(3),// AssigneeEmail
                reader.GetString(4),// Title
                reader.GetString(5),// Description
                Convert.ToDateTime(reader.GetString(6)),// DueDate
                Convert.ToDateTime(reader.GetString(7))// CreationTime
            );
        }
    }
}