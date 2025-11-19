using System;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTO
{
    internal class TaskDTO
    {
        public const string tasksIDColumnName = "TaskId";
        public const string tasksBoardIDColumnName = "BoardId";
        public const string tasksOrdColumnName = "Ordinal";
        public const string tasksTitleColumnName = "Title";
        public const string tasksDescriptionColumnName = "Description";
        public const string tasksDueDateColumnName = "DueDate";
        public const string tasksCreationTimeColumnName = "CreationTime";
        public const string tasksAssigneeColumnName = "AssigneeEmail";

        private readonly TaskController controller;
        private bool _isPersisted = false;

        public int TaskId { get; private set; }
        public int BoardID { get; private set; }

        private int _columnOrdinal;
        public int ColumnOrdinal
        {
            get => _columnOrdinal;
            set
            {

                _columnOrdinal = value;
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (_isPersisted)
                    controller.Update(TaskId, BoardID, tasksTitleColumnName, value);
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                if (_isPersisted)
                    controller.Update(TaskId, BoardID, tasksDescriptionColumnName, value);
            }
        }

        private DateTime _creationTime;
        public DateTime CreationTime
        {
            get => _creationTime;
            set
            {
                _creationTime = value;
                if (_isPersisted)
                    controller.Update(TaskId, BoardID, tasksCreationTimeColumnName, value.ToString("s"));
            }
        }

        private DateTime _dueDate;
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                if (_isPersisted)
                    controller.Update(TaskId, BoardID, tasksDueDateColumnName, value.ToString("s"));
            }
        }

        private string _assignee;
        public string Assignee
        {
            get => _assignee;
            set
            {
                _assignee = value;
                if (_isPersisted)
                    controller.Update(TaskId, BoardID, tasksAssigneeColumnName, value);
            }
        }

        public TaskDTO(int boardId, int ordinal, int taskId, string assignee, string title, string description, DateTime dueDate, DateTime creationTime)
        {
            controller = new TaskController();
            TaskId = taskId;
            BoardID = boardId;
            _columnOrdinal = ordinal;
            _assignee = assignee;
            _title = title;
            _description = description;
            _dueDate = dueDate;
            _creationTime = creationTime;
        }

        public void MarkAsPersisted()
        {
            _isPersisted = true;
        }
    }
}
