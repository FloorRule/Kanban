
using System;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class TaskBL
    {
        public const int MaxTitleLength = 50;
        public const int MaxDescriptionLength = 300;

        public long taskId;
        private readonly DateTime creationDate;
        private DateTime dueDate;
        private string? title;
        private string? description;

        public TaskDTO taskDTO { get; set; }
        private string assignee;

        public string Assignee
        {
            get => assignee;
            set
            {
                if (taskDTO != null)
                    taskDTO.Assignee = value;
                assignee = value; 
            }
        }
        public DateTime CreationDate
        {
            get => creationDate;
        }

        public string? Title
        {
            get => title;
            set
            {
                if (value == null)
                    throw new Exception("title cannot be null");
                if (value != null && value.Length > MaxTitleLength)
                    throw new Exception("title cannot exceed " + MaxTitleLength + " characters");
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("Task title is invalid");

                if (taskDTO != null)
                    taskDTO.Title = value;
                title = value;
            }
        }

        public string? Description
        {
            get => description;
            set
            {
                if (value == null)
                    throw new Exception("description cannot be null");
                if (value != null && value.Length > MaxDescriptionLength)
                    throw new Exception("description cannot exceed " + MaxDescriptionLength + " characters");
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("Task description is invalid");
                if (taskDTO != null)
                    taskDTO.Description = value;
                description = value;
            }
        }

        public DateTime DueDate
        {
            get => dueDate;
            set
            {
                if (value < DateTime.Now)
                    throw new Exception("Due date must not be in the past");
                if (taskDTO != null)
                    taskDTO.DueDate = value;
                dueDate = value;
            }
        }

        public TaskBL(long taskId, DateTime creationDate, DateTime dueDate, string title, string description, TaskDTO taskDto)
        {
            this.taskId = taskId;
            this.creationDate = creationDate;
            this.DueDate = dueDate;
            this.Title = title;
            this.Description = description;
            this.Assignee = "";
            this.taskDTO = taskDto;
        }


        public string UpdateTask(string email, SystemAction updatedField, object updatedValue)
        {
            if (email != this.assignee && this.assignee.Length != 0)
                throw new Exception("User is not the Assignee");
            switch (updatedField)
            {
                case SystemAction.UpdateTitle:
                    if (updatedValue is not string)
                        throw new Exception("Expected string for title");
                    
                    this.Title = (string)updatedValue;
                    break;
                case SystemAction.UpdateDescription:
                    if (updatedValue is not string)
                        throw new Exception("Expected string for description");

                    this.Description = (string)updatedValue;
                    break;
                case SystemAction.UpdateDueDate:
                    if (updatedValue is not DateTime)
                        throw new Exception("Expected DateTime for DueDate");

                    this.DueDate = (DateTime)updatedValue;
                    break;
                default:
                    throw new Exception("Field doesnt exist");
            }
            return "success";
        }

        public void AssigneTask(string email, string emailAssigne)
        {
            if (this.Assignee != "" && this.Assignee != email)
                throw new Exception("Task cannot be assigned by this user");
            this.Assignee = emailAssigne;
        }

    }
}

