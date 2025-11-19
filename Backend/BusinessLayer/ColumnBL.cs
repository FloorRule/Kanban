
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class ColumnBL
    {
        private int taskLimit = -1;
        private LinkedList<TaskBL> tasks;

        public ColumnDTO columnDTO;

        public int TaskLimit
        {
            get => taskLimit;
            set
            {
                if (value <= 0 && value != -1)
                    throw new Exception("Task limit must be positive");

                if (value < tasks.Count && value != -1)
                    throw new Exception("New limit is less than the current number of tasks.");

                if (columnDTO != null)
                    columnDTO.TasksLimit = value;
                taskLimit = value;
            }
        }
        public LinkedList<TaskBL> GetInProgressTasks(string email) 
        {
            LinkedList <TaskBL> userInProgTasks = new LinkedList<TaskBL> ();
            foreach (var task in tasks)
            {
               if(task.Assignee == email)
                    userInProgTasks.AddFirst (task);
            }
            return userInProgTasks; 
        }

        public LinkedList<TaskBL> Tasks => tasks;

        public ColumnBL(){
            this.tasks = new LinkedList<TaskBL>();
            this.columnDTO = null;
        }


        public void AddTask(string title, string desc, long taskId, DateTime todoDate, TaskDTO taskDto)
        {
            if (!AddTask(new TaskBL(taskId, DateTime.Now, todoDate, title, desc, taskDto)))
                throw new Exception("column is full");
        }

        public bool canTaskBeAdded()
        {
            return taskLimit == -1 || tasks.Count + 1 <= taskLimit;
        }

        public bool AddTask(TaskBL task)
        {
            if(canTaskBeAdded())
                tasks.AddLast(task);
            else
                return false;
            return true;
        }

        public bool RemoveTask(TaskBL task)
        {
            return tasks.Remove(task);
        }

        public TaskBL RemoveTask(long taskId)
        {
            foreach(TaskBL task in tasks)
            {
                if (task.taskId == taskId)
                {
                    RemoveTask(task);
                    return task;
                }
            }
                
            return null;
        }

        public void MakeAllTasksUnassigned(string email)
        {
            foreach (TaskBL task in tasks)
            {
                if(task.Assignee == email)
                    task.Assignee = "";
            }
        }

        public bool IsUserAssignee(string email, long taskId)
        {
            foreach(TaskBL task in tasks)
            {
                if (task.taskId == taskId)
                    return task.Assignee == email || task.Assignee.Length == 0 ;
            }
            return false;
        }

        public bool ContainsTask(long taskId)
        {
            foreach (TaskBL task in tasks)
            {
                if (task.taskId == taskId)
                    return true;

            }
            return false;
        }
        public string UpdateTask(string email, int taskId, SystemAction updatedField, object updatedValue)
        {
            foreach (TaskBL task in tasks)
            {
                if (task.taskId == taskId)
                    return task.UpdateTask(email, updatedField, updatedValue);
            }
            throw new Exception("Update Task failed");
        }

        public string AssigneTask(string email, string emailAssigne, long taskId)
        {
            foreach (TaskBL task in tasks)
            {
                if (task.taskId == taskId)
                {
                    task.AssigneTask(email, emailAssigne);
                    return "success";
                }
                    
            }
            throw new Exception("Assignee Task failed");
        }





    }
}