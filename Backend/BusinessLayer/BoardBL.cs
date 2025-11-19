
using System;
using System.Collections.Generic;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardBL
    {
        private readonly string name;
        private readonly Dictionary<ColumnType, ColumnBL> boardColumns;

        private int boardId;
        private Dictionary<string, bool> boardMembers;
        public BoardDTO boardDTO;

        public int BoardId
        {
            get => boardId;
        }

        public Dictionary<string, bool> BoardMembers
        {
            get => boardMembers;
        }

        public Dictionary<ColumnType, ColumnBL> BoardColumns{
            get => boardColumns;
        }

        public string Name
        {
            get => name;
        }

        public BoardBL(string name, int boardId)
        {
            this.name = name;
            this.boardColumns = new Dictionary<ColumnType, ColumnBL>{
                { ColumnType.BackLog, new ColumnBL() },
                { ColumnType.InProgress, new ColumnBL() },
                { ColumnType.Done, new ColumnBL() }
            };
            this.boardMembers = new Dictionary<string, bool>();
            this.boardId = boardId;
            this.boardDTO = null;
        }

        public void LimitColumn(ColumnType type, int newTaskLimit)
        {
            boardColumns[type].TaskLimit = newTaskLimit;
        }

        public string AddTask(string email, string title, string description, DateTime todoDate, long taskId, TaskDTO taskDto, ColumnType source = ColumnType.BackLog)
        {
            if(!IsBoardMember(email))
                throw new Exception("User is not a BoardMember");
            boardColumns[source].AddTask( title, description , taskId, todoDate, taskDto);
            return "success";
        }


        public ColumnType FindTaskColumn(long taskId)
        {
            for (ColumnType column = ColumnType.BackLog; column <= ColumnType.Done; column++)
            {
                if (boardColumns[column].ContainsTask(taskId))
                    return column;
            }

            throw new Exception("Task doesnt exist");
        }
        public TaskBL RemoveTask(long taskId)
        {
            ColumnType col = FindTaskColumn(taskId);
           
            if (col == ColumnType.Done)
                throw new Exception("Cannot remove task from Done column");

            TaskBL task = boardColumns[col].RemoveTask(taskId);
            if (task == null)
                throw new Exception("Task doesnt exist");

            return task;
        }

        public string MoveTaskForwards(string email, long taskId)
        {
            
            ColumnType col = FindTaskColumn(taskId);

            if (col == ColumnType.Done)
                throw new Exception("Task is already in Done column");

            if (!boardColumns[col].canTaskBeAdded())
                throw new Exception("column is full");

            if(!boardColumns[col].IsUserAssignee(email, taskId))
                throw new Exception("User is not Assignee");

            TaskBL task = boardColumns[col].RemoveTask(taskId);
            
            ColumnType nextCol = col + 1;
            boardColumns[nextCol].AddTask(task);

            if (task.taskDTO != null)
                task.taskDTO.ColumnOrdinal = nextCol.GetHashCode();

            return "Task moved successfully";
        }

        public string UpdateTask(string email, int taskId, ColumnType columnNumber, SystemAction updatedField, object updatedValue)
        {
            if (columnNumber == ColumnType.Done)
                throw new Exception("Cannot Update task from Done column");

            return boardColumns[columnNumber].UpdateTask(email, taskId, updatedField, updatedValue);

        }

        public LinkedList<TaskBL> GetInProgressTasks(string email)
        {
            return boardColumns[ColumnType.InProgress].GetInProgressTasks(email); 
        }

        public int GetColumnLimit(ColumnType columnNumber)
        {
            return this.boardColumns[columnNumber].TaskLimit;
        }

        public string GetColumnName(ColumnType columnNumber)
        {
            if(columnNumber == ColumnType.InProgress)
                return "in progress";
            return columnNumber.ToString().ToLower();
        }

        public ColumnBL GetColumn(ColumnType columnNumber)
        {
            return this.boardColumns[columnNumber];
        }

        public void AssigneTask(string email, string emailAssignee, long taskId, ColumnType columnType)
        {
            if (!IsBoardMember(email))
                throw new Exception("User is not a BoardMember");
            if (!IsBoardMember(emailAssignee))
                throw new Exception("Assignee is not a BoardMember");

            this.boardColumns[columnType].AssigneTask(email, emailAssignee, taskId);
        }

        public bool IsBoardMember(string newOwnerEnail)
        {
            return this.boardMembers.ContainsKey(newOwnerEnail);
        }

        public string JoinBoard(string email)
        {
            if (IsBoardMember(email))
                throw new Exception("User is already a BoardMember");
            this.boardMembers.Add(email, true);
            return "User joined successfully";
        }

        public string LeaveBoard(string email)
        {
            if (!IsBoardMember(email))
                throw new Exception("User is not a BoardMember");

            this.boardColumns[ColumnType.BackLog].MakeAllTasksUnassigned(email);
            this.boardColumns[ColumnType.InProgress].MakeAllTasksUnassigned(email);

            this.boardMembers.Remove(email);
            return "User exited successfully";
        }
    }
}
