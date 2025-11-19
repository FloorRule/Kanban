using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class TaskService
    {
        private BoardFacade boardFacade;

        internal TaskService(BoardFacade boardFacade)
        {
            this.boardFacade = boardFacade;
        }

        /// <summary>
        /// Adds a new task to a specified board and column.
        /// 
        /// Preconditions: The email and board name must be valid. The column must exist. The task ID must be unique. Title must not be empty.
        /// Postconditions: A new task is created and added to the board. Returns a success message or task details.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="title">Title of the task.</param>
        /// <param name="desc">Description of the task.</param>
        /// <param name="taskId">The ID of the task to be added.</param>
        /// <param name="toDoDate">The due date of the task.</param>
        /// <returns>A success message or task ID.</returns>
        public string AddTask(string email, string boardName, string title, string description, DateTime todoDate)
        {
            Response res;
            try
            {
                long id = boardFacade.AddTask(email, boardName, title, description, todoDate);
                res = new Response(id);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        /// <summary>
        /// Removes a task by its ID from a specific board.
        /// 
        /// Preconditions: The task must exist and belong to the user and board specified.
        /// Postconditions: The task is removed. Returns a confirmation message.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="id">The ID of the task to remove.</param>
        /// <returns>A confirmation message.</returns>
        public string RemoveTask(string email, string boardName, int taskId)
        {
            Response res;
            try
            {
                this.boardFacade.RemoveTask(email, boardName, taskId);
                res = new Response();
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        /// <summary>
        /// Updates an existing task's title, description, and due date.
        /// 
        /// Preconditions: The task must exist and the user must have permission to modify it. Title must not be empty.
        /// Postconditions: The task's details are updated. Returns a confirmation message.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="title">The new title for the task.</param>
        /// <param name="desc">The new description for the task.</param>
        /// <param name="toDoDate">The new due date.</param>
        /// <returns>A confirmation message.</returns>
        private string UpdateTask(string email, string boardName, int columnNumber, int taskId, SystemAction updatedField, object updatedVaue)
        {
            Response res;
            try
            {
                this.boardFacade.UpdateTask(email, boardName, columnNumber, taskId, updatedField, updatedVaue);
                res = new Response();
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }

        }
        public string UpdateTaskDueDate(string email, string boardName, int columnNumber, int taskId, object updatedVaue)
        {
            return UpdateTask(email,boardName,columnNumber,taskId, SystemAction.UpdateDueDate, updatedVaue);
        }

        public string UpdateTaskTitle(string email, string boardName, int columnNumber, int taskId, object updatedVaue)
        {
            return UpdateTask(email, boardName, columnNumber, taskId, SystemAction.UpdateTitle, updatedVaue);
        }

        public string UpdateTaskDescription(string email, string boardName, int columnNumber, int taskId, object updatedVaue)
        {
            return UpdateTask(email, boardName, columnNumber, taskId, SystemAction.UpdateDescription, updatedVaue);
        }

        /// <summary>
        /// Moves a task to the next column in the workflow.
        /// 
        /// Preconditions: The task must exist, and the user must have permission to move it. There must be a next column available.
        /// Postconditions: The task is moved to the next column. Returns a confirmation message.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="id">The ID of the task to move.</param>
        /// <returns>A success or error message.</returns>
        public string AdvanceTask(string email, string boardName, int taskId)
        {
            Response res;
            try
            {
                this.boardFacade.MoveTaskForwards(email, boardName, taskId);
                res = new Response();
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }


        /// <summary>
        /// Retrieves all tasks that are currently in progress for a board.
        /// 
        /// Preconditions: The user must be a member of the board.
        /// Postconditions: Returns a list of tasks in the 'In Progress' column.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <returns>A string listing in-progress tasks.</returns>
        public string InProgressTasks(string email)
        {
            Response res;
            try
            {
                LinkedList<TaskBL> inProgressTasks = boardFacade.GetInProgressTasks(email);
                res = new Response(Convertor.taskConverterSL(inProgressTasks));
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
           
        }

        /// <summary>
        /// Assigns a task to another user within the specified column.
        /// 
        /// Preconditions: The task must exist, the user assigning must have permission, the assignee must be a board member, and the task must be in the specified column.
        /// Postconditions: The task is assigned to the new user. Returns a confirmation or error message.
        /// </summary>
        /// <param name="email">Email of the user requesting the assignment.</param>
        /// <param name="boardName">The name of the board containing the task.</param>
        /// <param name="emailAssigne">Email of the user to whom the task will be assigned.</param>
        /// <param name="taskId">The ID of the task to assign.</param>
        /// <param name="columnType">The column in which the task currently resides.</param>
        /// <returns>A success or error message.</returns>
        /// <exception cref="NotImplementedException"></exception>

        public string AssigneTask(string email, string boardName, string emailAssigne, long taskId, int columnType)
        {
            Response res;
            try
            {
                boardFacade.AssigneTask(email, boardName, emailAssigne, taskId, columnType);
                res = new Response();
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }

        }


    }
}
