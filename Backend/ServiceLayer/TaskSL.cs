using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class TaskSL
    {
        [JsonPropertyName("Id")]
        public long Id { get; }

        [JsonPropertyName("CreationTime")]
        public DateTime CreationTime { get; }

        [JsonPropertyName("DueDate")]
        public DateTime DueDate { get; }

        [JsonPropertyName("Title")]
        public string? Title { get; }

        [JsonPropertyName("Description")]
        public string? Description { get; }

        [JsonPropertyName("Assignee")]
        public string Assignee { get; }

        internal TaskSL(TaskBL taskBL)
        {
            this.Id = taskBL.taskId;
            this.CreationTime = taskBL.CreationDate;
            this.DueDate = taskBL.DueDate;
            this.Title = taskBL.Title;
            this.Description = taskBL.Description;
            this.Assignee = taskBL.Assignee;
        }

        [JsonConstructor]
        public TaskSL(long Id, DateTime CreationTime, DateTime DueDate, string Title, string Description, string Assignee)
        {
            this.Id = Id;
            this.CreationTime = CreationTime;
            this.DueDate = DueDate;
            this.Title = Title;
            this.Description = Description;
            this.Assignee = Assignee;
        }


    }
}
