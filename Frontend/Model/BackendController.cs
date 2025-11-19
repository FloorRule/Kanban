using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.ServiceLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Frontend.Model
{
    public class BackendController
    {
        private FactoryService FactoryService { get; set; }

        public BackendController(FactoryService FactoryService)
        {
            this.FactoryService = FactoryService;
        }
        public BackendController() 
        {
            TestDbBoot.EnsureSchemaExists();
            this.FactoryService = new FactoryService();
            this.FactoryService.LoadData();
        }

        // --- User Methods ---
        public UserModel Login(string email, string password)
        {
            string jsonResponse = FactoryService.userService.Login(email, password);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);

            return new UserModel(this, email);
        }

        public UserModel Register(string email, string password)
        {
            string jsonResponse = FactoryService.userService.Register(email, password);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);

            return new UserModel(this, email);
        }

        public void Logout(string email)
        {
            string jsonResponse = FactoryService.userService.Logout(email);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }

        // --- Board Methods ---

        public void CreateBoard(UserModel user, string boardName)
        {
            string jsonResponse = FactoryService.boardService.CreateBoard(user.Email, boardName);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }

        public void DeleteBoard(UserModel user, string boardName)
        {
            string jsonResponse = FactoryService.boardService.DeleteBoard(user.Email, boardName);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }

        public void JoinBoard(UserModel user, int boardID)
        {
            string jsonResponse = FactoryService.boardService.JoinBoard(user.Email, boardID);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }
        public void LeaveBoard(UserModel user, int boardId)
        {
            string jsonResponse = FactoryService.boardService.LeaveBoard(user.Email, boardId);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }

        public void TransferOwnership(UserModel user, string newOwnerEmail, string boardName)
        {
            string jsonResponse = FactoryService.boardService.TransferOwnerShip(user.Email, newOwnerEmail, boardName);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }

        public BoardModel GetBoard(UserModel user, int boardId)
        {
            string boardNameJson = FactoryService.boardService.GetBoardName(boardId);
            Response nameRes = JsonSerializer.Deserialize<Response>(boardNameJson);
            if (nameRes.ErrorOccurred)
                throw new Exception(nameRes.ErrorMessage);

            string boardName = nameRes.ReturnValue.ToString();

            string ownerEmailJson = FactoryService.boardService.GetBoardOwner(boardId);
            Response ownerRes = JsonSerializer.Deserialize<Response>(ownerEmailJson);
            if (ownerRes.ErrorOccurred)
                throw new Exception(ownerRes.ErrorMessage);

            string ownerEmail = ownerRes.ReturnValue.ToString();

            UserModel ownerModel = new UserModel(this, ownerEmail);

            string boardMembersJson = FactoryService.boardService.GetAllBoardMembers(user.Email, boardName);
            Response membersRes = JsonSerializer.Deserialize<Response>(boardMembersJson);
            if (membersRes.ErrorOccurred)
                throw new Exception(membersRes.ErrorMessage);
            List<string> boardMembers = JsonSerializer.Deserialize<List<string>>(membersRes.ReturnValue.ToString());

            return new BoardModel(this, user, ownerModel, boardId, boardName, boardMembers);
        }

        public List<BoardModel> GetUserBoards(UserModel user)
        {
            string jsonResponse = FactoryService.boardService.GetUserBoards(user.Email);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);

            // list of board IDs.
            List<int> boardIds = JsonSerializer.Deserialize<List<int>>(res.ReturnValue.ToString());

            // full board details
            List<BoardModel> boards = new List<BoardModel>();
            foreach (int id in boardIds)
                boards.Add(GetBoard(user, id));

            return boards;
        }

        public List<BoardModel> GetAllBoards(UserModel user)
        {
            string jsonResponse = FactoryService.boardService.GetAllBoards(user.Email);

            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);

            // list of board IDs of each user.
            Dictionary<string, List<int>> boardInfos = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(res.ReturnValue.ToString());

            // full board details
            List<BoardModel> boards = new List<BoardModel>();
            foreach (var userBoard in boardInfos)
                foreach (int id in userBoard.Value)
                {
                    BoardModel boardModel = GetBoard(new UserModel(this, userBoard.Key), id);
                    boardModel.CheckJoinability(user.Email);
                    boards.Add(boardModel);
                }
                    

            return boards;
        }

        // --- Column & Task Methods ---

        public List<TaskModel> GetColumnTasks(UserModel user, string boardName, int columnOrdinal)
        {
            string jsonResponse = FactoryService.boardService.GetColumn(user.Email, boardName, columnOrdinal);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);

            List<TaskSL> serviceTasks = JsonSerializer.Deserialize<List<TaskSL>>(res.ReturnValue.ToString());

            // THE FIX: Pass the 'user' parameter to the TaskModel constructor here.
            return serviceTasks.Select(st => new TaskModel(this, st, boardName, columnOrdinal, user)).ToList();
        }

        public void AddTask(string email, string boardName, string title, string description, DateTime dueDate)
        {
            string jsonResponse = FactoryService.taskService.AddTask(email, boardName, title, description, dueDate);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred) 
                throw new Exception(res.ErrorMessage);
        }

        public void UpdateTask(string email, string boardName, int columnOrdinal, int taskId, SystemAction action, object value)
        {
            string jsonResponse = "";
            switch (action)
            {
                case SystemAction.UpdateTitle:
                    jsonResponse = FactoryService.taskService.UpdateTaskTitle(email, boardName, columnOrdinal, taskId, value);
                    break;
                case SystemAction.UpdateDescription:
                    jsonResponse = FactoryService.taskService.UpdateTaskDescription(email, boardName, columnOrdinal, taskId, value);
                    break;
                case SystemAction.UpdateDueDate:
                    jsonResponse = FactoryService.taskService.UpdateTaskDueDate(email, boardName, columnOrdinal, taskId, value);
                    break;
            }

            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred) 
                throw new Exception(res.ErrorMessage);
        }

        public void MoveTask(string email, string boardName, int taskId)
        {
            string jsonResponse = FactoryService.taskService.AdvanceTask(email, boardName, taskId);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }

        public void DeleteTask(string email, string boardName, int taskId)
        {
            string jsonResponse = FactoryService.taskService.RemoveTask(email, boardName, taskId);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);

            if (res.ErrorOccurred) 
                throw new Exception(res.ErrorMessage);
        }

        public void AssignTask(string userEmail, string boardName, int columnOrdinal, int taskId, string newAssigneeEmail)
        {
            string jsonResponse = FactoryService.taskService.AssigneTask(userEmail, boardName, newAssigneeEmail, taskId, columnOrdinal);
            Response res = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (res.ErrorOccurred)
                throw new Exception(res.ErrorMessage);
        }
        
    }
}
