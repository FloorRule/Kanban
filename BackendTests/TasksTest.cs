
﻿using System.Text.Json;
using IntroSE.Kanban.Backend.ServiceLayer;


namespace BackendTests
{
    public class TasksTest
    {
        private readonly TaskService taskService;
        private readonly BoardService boardService;
        private readonly UserService userService;


        public TasksTest(FactoryService factory)
        {
            this.taskService = factory.taskService;
            this.boardService = factory.boardService;
            this.userService = factory.userService;
        }

        public void RunAllTests()
        {
            // Setup users and board
            userService.Register("ali@gmail.com", "Password123");
            userService.Login("ali@gmail.com", "Password123");
            boardService.CreateBoard("ali@gmail.com", "house");

            AddTaskTests();
            UpdateTaskTests();
            MoveAndRemoveTaskTests();
            AssignTaskTests();
            InProgressTasksTests();
        }

        #region Add Task Tests

        public void AddTaskTests()
        {
            Console.WriteLine("\n------ Add Task Tests ------");

            // This is a valid test
            PrintResult("Add valid task", taskService.AddTask("ali@gmail.com", "house", "cleaning the room", "must clean the room", DateTime.Now.AddDays(5)));

            // This is an invalid test (empty title)
            PrintResult("Add task with empty title", taskService.AddTask("ali@gmail.com", "house", "", "must clean the room", DateTime.Now.AddDays(5)));

            // this title is too long
            PrintResult("Add task with long title", taskService.AddTask("ali@gmail.com", "house", new string('a', 51), "must clean the room", DateTime.Now.AddDays(5)));

            // this description is too long
            PrintResult("Add task with long description", taskService.AddTask("ali@gmail.com", "house", "cleaning", new string('a', 301), DateTime.Now.AddDays(5)));

            // this date is from the past
            PrintResult("Add task with past due date", taskService.AddTask("ali@gmail.com", "house", "cleaning", "must clean the room", DateTime.Now.AddDays(-1)));

            // this description is empty
            PrintResult("Add task with empty description", taskService.AddTask("ali@gmail.com", "house", "cleaning", "", DateTime.Now.AddDays(5)));
        }

        #endregion

        #region Update Task Tests

        public void UpdateTaskTests()
        {
            Console.WriteLine("\n------ Update Task Tests ------");
            long taskId = GetTaskIdFromResponse(taskService.AddTask("ali@gmail.com", "house", "updatable task", "desc", DateTime.Now.AddDays(5)));
            if (taskId == -1) 
                return;

            // valid updates
            PrintResult("Valid update (title)", taskService.UpdateTaskTitle("ali@gmail.com", "house", 0, (int)taskId, "new title"));
            PrintResult("Valid update (description)", taskService.UpdateTaskDescription("ali@gmail.com", "house", 0, (int)taskId, "updated desc"));
            PrintResult("Valid update (due date)", taskService.UpdateTaskDueDate("ali@gmail.com", "house", 0, (int)taskId, DateTime.Now.AddDays(10)));

            // invalid updates
            PrintResult("Update with empty title", taskService.UpdateTaskTitle("ali@gmail.com", "house", 0, (int)taskId, ""));
            PrintResult("Update with long title", taskService.UpdateTaskTitle("ali@gmail.com", "house", 0, (int)taskId, new string('a', 51)));
            PrintResult("Update with long desc", taskService.UpdateTaskDescription("ali@gmail.com", "house", 0, (int)taskId, new string('a', 301)));
            PrintResult("Update with past due date", taskService.UpdateTaskDueDate("ali@gmail.com", "house", 0, (int)taskId, DateTime.Now.AddDays(-2)));

            // Move to Done and attempt to update (should fail)
            taskService.AdvanceTask("ali@gmail.com", "house", (int)taskId); // Backlog -> InProgress
            taskService.AdvanceTask("ali@gmail.com", "house", (int)taskId); // InProgress -> Done
            PrintResult("Update Done task (should fail)", taskService.UpdateTaskTitle("ali@gmail.com", "house", 2, (int)taskId, "title in done"));
        }

        #endregion

        #region Move and Remove Tests
        public void MoveAndRemoveTaskTests()
        {
            Console.WriteLine("\n------ Move and Remove Task Tests ------");
            long taskId = GetTaskIdFromResponse(taskService.AddTask("ali@gmail.com", "house", "removable task", "desc", DateTime.Now.AddDays(3)));
            if (taskId == -1) 
                return;

            PrintResult($"Move task {taskId} to InProgress", taskService.AdvanceTask("ali@gmail.com", "house", (int)taskId));
            PrintResult($"Remove existing task {taskId}", taskService.RemoveTask("ali@gmail.com", "house", (int)taskId));
            PrintResult("Remove non-existent task", taskService.RemoveTask("ali@gmail.com", "house", 999));
        }
        #endregion

        #region Assign Task Tests
        public void AssignTaskTests()
        {
            Console.WriteLine("\n------ Assign Task Tests ------");

            // Setup
            userService.Register("aya@gmail.com", "Password123");
            userService.Login("aya@gmail.com", "Password123");
            boardService.CreateBoard("ali@gmail.com", "shared_board");
            int boardId = GetFirstBoardId("ali@gmail.com", "shared_board");
            boardService.JoinBoard("aya@gmail.com", boardId);
            long taskId = GetTaskIdFromResponse(taskService.AddTask("ali@gmail.com", "shared_board", "task to assign", "d", DateTime.Now.AddDays(1)));
            if (taskId == -1) 
                return;

            // Tests
            PrintResult("Assign task to a non-member (fail)", taskService.AssigneTask("ali@gmail.com", "shared_board", "noone@gmail.com", taskId, 0));
            PrintResult("Assign valid task to valid user (success)", taskService.AssigneTask("ali@gmail.com", "shared_board", "aya@gmail.com", taskId, 0));
            PrintResult("Assign task not as owner (fail)", taskService.AssigneTask("aya@gmail.com", "shared_board", "ali@gmail.com", taskId, 0));
        }

        #endregion

        #region InProgress Tests

        public void InProgressTasksTests()
        {
            Console.WriteLine("\n------ In-Progress Tasks Tests ------");
            long task1Id = GetTaskIdFromResponse(taskService.AddTask("ali@gmail.com", "house", "Task A", "d", DateTime.Now.AddDays(1)));
            long task2Id = GetTaskIdFromResponse(taskService.AddTask("ali@gmail.com", "house", "Task B", "d", DateTime.Now.AddDays(1)));
            long task3Id = GetTaskIdFromResponse(taskService.AddTask("ali@gmail.com", "house", "Task C", "d", DateTime.Now.AddDays(1)));

            if (task1Id == -1 || task2Id == -1) 
                return;

            taskService.AssigneTask("ali@gmail.com", "house", "ali@gmail.com", task1Id, 0);
            taskService.AssigneTask("ali@gmail.com", "house", "ali@gmail.com", task2Id, 0);

            // Move two tasks to InProgress
            taskService.AdvanceTask("ali@gmail.com", "house", (int)task1Id);
            taskService.AdvanceTask("ali@gmail.com", "house", (int)task2Id);

            PrintResult("Get In-Progress tasks", taskService.InProgressTasks("ali@gmail.com"));
        }

        #endregion

        #region Helper Methods

        private void PrintResult(string label, string json)
        {
            try
            {
                Response? response = JsonSerializer.Deserialize<Response>(json);
                if (response.ErrorOccurred)
                {
                    Console.WriteLine($"{label}: Failed - {response.ErrorMessage}");
                }
                else
                {
                    string returnValue = response.ReturnValue != null ? response.ReturnValue.ToString() : "void";
                    Console.WriteLine($"{label}: Succeeded - ReturnValue: {returnValue}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error deserializing response for '{label}': {ex.Message}");
            }
        }

        private long GetTaskIdFromResponse(string json)
        {
            Response? response = JsonSerializer.Deserialize<Response>(json);
            if (!response.ErrorOccurred && response.ReturnValue is JsonElement element && element.TryGetInt64(out long id))
            {
                return id;
            }
            Console.WriteLine("Failed to get task ID from response.");
            return -1;
        }

        private int GetFirstBoardId(string email, string boardName)
        {
            // This is a simplified helper; a more robust solution would be needed in a real app
            // For now, it assumes the latest board created is the one we want.
            string jsonResponse = boardService.GetUserBoards(email);
            Response? response = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (!response.ErrorOccurred && response.ReturnValue != null)
            {
                JsonElement returnValue = (JsonElement)response.ReturnValue;
                List<int> boardIds = JsonSerializer.Deserialize<List<int>>(returnValue.GetRawText());
                return boardIds.LastOrDefault(-1);
            }
            return -1;
        }

        #endregion
    }
}