
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTO;
using IntroSE.Kanban.Backend.ServiceLayer;
using log4net;
using log4net.Config;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardFacade
    {
        private readonly Dictionary<string, Dictionary<(string, int), BoardBL>> boards = new Dictionary<string, Dictionary<(string, int), BoardBL>>();
        private long taskIdControl;
        private readonly AuthenticationFacade authenticationFacade;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int boardIDCounter;

        private readonly BoardController _boardController = new BoardController();
        private readonly ColumnController _columnController = new ColumnController();
        private readonly TaskController _taskController = new TaskController();
        private readonly BoardMembersController _boardMembersController = new BoardMembersController();
        public BoardFacade(AuthenticationFacade authenticationFacade) {
            this.taskIdControl = 0;
            this.authenticationFacade = authenticationFacade;

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            log.Info("- BoardFacade Log Initialized -");

            this.boardIDCounter = 0;
        }

        private string handleBoardName(string boardName)
        {
            if (string.IsNullOrWhiteSpace(boardName))
                ThrowAndLog("BoardName is invalid"); //Project Alpha != Project ALpha
            // VPL, Project Alpha -> handleBoardName("Project Alpha") -> project alpha, Project Alpha == project alpha -> Flse
            //boardName = boardName.ToLower(); 
            boardName = boardName.Trim();
            return boardName;
        }

        private void validateLoggedIn(string email)
        {
            if (!authenticationFacade.IsLoggedIn(email))
                ThrowAndLog("User is not logged in");
        }

        private void validateExists(string email)
        {
            authenticationFacade.IsLoggedIn(email);
        }

        private void validateUserBoardListExists(string email)
        {
            if (!boards.ContainsKey(email))
                ThrowAndLog("User is not on board list");
        }

        private void validateBoardExists(string email, string boardName)
        {
            if (!boards[email].Keys.Any(k => k.Item1 == boardName))
                ThrowAndLog("Board with that name doesn't exist");
        }

        private ColumnType validateColumnType(int columnNumber)
        {
            if (!Enum.IsDefined(typeof(ColumnType), columnNumber))
                ThrowAndLog("Invalid column ordinal");
            return (ColumnType)columnNumber;
        }

        private void validateBoardMember(string email, BoardBL boardToTransfer)
        {
            if (!boardToTransfer.IsBoardMember(email))
                ThrowAndLog("New Owner is not a board member");
        }

        private void tryInitializeUserDict(string email)
        {
            if (!this.boards.ContainsKey(email))
                this.boards.Add(email, new Dictionary<(string, int), BoardBL>());
        }
        
        private void tryRemoveUserBoard(string email, (string, int) key)
        {
            if (!this.boards[email].Remove(key))
                ThrowAndLog("BoardName with that name doesnt exists");
        }

        private void ThrowAndLog(string message)
        {
            log.Warn(message);
            throw new Exception(message);
        }

        public void CreateBoard(string email, string boardName)
        {
            validateLoggedIn(email);

            boardName = handleBoardName(boardName);

            tryInitializeUserDict(email);

            if (this.boards[email].Keys.Any(k => k.Item1 == boardName))
                ThrowAndLog("BoardName with that name already exists");

            this.boardIDCounter++;
            int newBoardId = this.boardIDCounter;

            var boardDto = new BoardDTO(newBoardId, boardName, email);
            _boardController.Insert(boardDto);
            boardDto.MarkAsPersisted();

            BoardBL boardBL = new BoardBL(boardName, boardIDCounter) { boardDTO = boardDto };

            for (ColumnType ord = ColumnType.BackLog; ord <= ColumnType.Done; ord++)
            {
                var colDto = new ColumnDTO(boardIDCounter, ord.GetHashCode(), -1);
                _columnController.Insert(colDto);
                colDto.MarkAsPersisted();
            }

            

            boardBL.BoardColumns[ColumnType.BackLog].columnDTO = new ColumnDTO(boardIDCounter, ColumnType.BackLog.GetHashCode(), -1);
            boardBL.BoardColumns[ColumnType.InProgress].columnDTO = new ColumnDTO(boardIDCounter, ColumnType.InProgress.GetHashCode(), -1);
            boardBL.BoardColumns[ColumnType.Done].columnDTO = new ColumnDTO(boardIDCounter, ColumnType.Done.GetHashCode(), -1);

            this.boards[email].Add((boardName, newBoardId), boardBL);
            _boardMembersController.Insert(new BoardMemberDTO(newBoardId, email));
            boardBL.JoinBoard(email);
        }

        public ((string, int), BoardBL) DeleteBoard(string email, string boardName)
        {
            validateLoggedIn(email);

            boardName = handleBoardName(boardName);

            validateUserBoardListExists(email);

            validateBoardExists(email, boardName);

            var keyToRemove = this.boards[email].Keys.FirstOrDefault(k => k.Item1 == boardName);

            ((string, int), BoardBL) removedBoard = new(keyToRemove, this.boards[email][keyToRemove]);

            tryRemoveUserBoard(email, keyToRemove);

            _taskController.DeleteAll();
            _columnController.Delete(new ColumnDTO(removedBoard.Item2.BoardId, 0, 0));
            _boardController.Delete(removedBoard.Item2.boardDTO);

            return removedBoard;

        }

        private void AddBoard(string email, ((string, int), BoardBL) boardToAdd)
        {
            this.boards[email].Add(boardToAdd.Item1, boardToAdd.Item2);
        }

        public void TransferOwnerShip(string currentOwnerEmail, string newOwnerEmail, string boardName)
        {
            validateLoggedIn(currentOwnerEmail);
            boardName = handleBoardName(boardName);
            validateUserBoardListExists(currentOwnerEmail);

            validateBoardExists(currentOwnerEmail, boardName);

            //validateExists(newOwnerEmail);

            var currentOwnerBoard = this.boards[currentOwnerEmail].FirstOrDefault(entry => entry.Key.Item1 == boardName);

            BoardBL boardToTransfer = currentOwnerBoard.Value;
            var boardKey = currentOwnerBoard.Key;

            validateBoardMember(newOwnerEmail, boardToTransfer);

            tryRemoveUserBoard(currentOwnerEmail, boardKey);

            boardToTransfer.boardDTO.OwnerEmail = newOwnerEmail;  

            if (this.boards[currentOwnerEmail].Count == 0)
                this.boards.Remove(currentOwnerEmail);

            tryInitializeUserDict(newOwnerEmail);

            if (this.boards[newOwnerEmail].ContainsKey(boardKey))
                ThrowAndLog("New owner already appears to own board");

            
            AddBoard(newOwnerEmail, (boardKey,boardToTransfer));
        }

        public void JoinBoard(string email, int boardID)
        {
            validateLoggedIn(email);
            validateExists(email);

            foreach (var userBoards in this.boards.Values)
            {
                var match = userBoards.FirstOrDefault(entry => entry.Key.Item2 == boardID);
                if (!match.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    match.Value.JoinBoard(email);
                    _boardMembersController.Insert(new BoardMemberDTO(boardID, email));
                    return;
                }
                    
            }
            ThrowAndLog("Board with that boardID doesnt exists");
        }

        public void LeaveBoard(string email, int boardID)
        {
            validateLoggedIn(email);

            foreach (var ownerEmail in this.boards.Keys)
            {
                var userOwnedBoards = this.boards[ownerEmail];
                var match = userOwnedBoards.FirstOrDefault(entry => entry.Key.Item2 == boardID);
                if (!match.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    if (ownerEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                        ThrowAndLog("Board owner cannot leave the board. Transfer ownership first");

                    match.Value.LeaveBoard(email);
                    _boardMembersController.Delete(new BoardMemberDTO(boardID, email));
                    return;
                }
            }
            ThrowAndLog("Board with that boardID doesnt exists");
        }

        public void LimitColumn(string email, string boardName, int columnType, int newTaskLimit)
        {
            ColumnType selectedColumntype = validateColumnType(columnType);

            validateLoggedIn(email);

            if (newTaskLimit < -1)
                ThrowAndLog("Task limit must be positive");

            boardName = handleBoardName(boardName);

            validateUserBoardListExists(email);
            validateBoardExists(email, boardName);

            var selectedBoard = boards[email].FirstOrDefault(kvp => kvp.Key.Item1 == boardName);
            if(!selectedBoard.Equals(default(KeyValuePair<(string, int), BoardBL>)))
            {
                _columnController.Update(selectedBoard.Key.Item2, columnType, "TasksLimit", newTaskLimit);
                selectedBoard.Value.LimitColumn(selectedColumntype, newTaskLimit);
            }
        }

        public long AddTask(string email, string boardName, string title, string description, DateTime todoDate)
        {
            validateLoggedIn(email);
            boardName = handleBoardName(boardName);

            BoardBL targetBoard = null;
            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));
                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    targetBoard = boardEntry.Value;
                    break;
                }
            }

            if (targetBoard == null)
                ThrowAndLog("Board not found or user is not a member");

            long id = taskIdControl++;

            TaskDTO taskDto = new TaskDTO(targetBoard.BoardId, ColumnType.BackLog.GetHashCode(), (int)id, "", title, description, todoDate, DateTime.Now);

            targetBoard.AddTask(email, title, description, todoDate, id, taskDto);

            _taskController.Insert(taskDto);
            taskDto.MarkAsPersisted();
            return id;
        }

        public void RemoveTask(string email, string boardName, long taskId)
        {
            validateLoggedIn(email);
            boardName = handleBoardName(boardName);

            BoardBL board = null;
            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));
                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    board = boardEntry.Value;
                    break;
                }
            }

            if (board == null)
                ThrowAndLog("Board not found or user is not a member");

            TaskBL taskToRemoved = board.RemoveTask(taskId);

            if (taskToRemoved == null)
                ThrowAndLog("Task with ID not found in board");

            if (taskToRemoved.taskDTO != null)
                _taskController.Delete(taskToRemoved.taskDTO);
        }

        public void MoveTaskForwards(string email, string boardName, long taskId)
        {
            validateLoggedIn(email);
            boardName = handleBoardName(boardName);

            BoardBL targetBoard = null;

            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));
                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    targetBoard = boardEntry.Value;
                    break;
                }
            }
            if (targetBoard == null)
                ThrowAndLog("Board with that name is not found or User is not a member");

            var columnType = targetBoard.FindTaskColumn(taskId);
            var taskToMove = targetBoard.BoardColumns[columnType].Tasks.First(t => t.taskId == taskId);

            _taskController.Delete(taskToMove.taskDTO);

            targetBoard.MoveTaskForwards(email, taskId);

            _taskController.Insert(taskToMove.taskDTO);
        }

        public void UpdateTask(string email, string boardName, int columnNumber, int taskId, SystemAction updatedField, object updatedValue)
        {
            ColumnType selectedColumntype = validateColumnType(columnNumber);

            validateLoggedIn(email);

            boardName = handleBoardName(boardName);

            BoardBL targetBoard = null;
            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));
                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    targetBoard = boardEntry.Value;
                    break;
                }
            }

            if (targetBoard == null)
                ThrowAndLog("Board not found or user is not a member");

            targetBoard.UpdateTask(email, taskId, selectedColumntype, updatedField, updatedValue);
        }

        public void AssigneTask(string email, string boardName, string emailAssigne, long taskId, int columnNumber)
        {
            ColumnType selectedColumntype = validateColumnType(columnNumber);

            validateLoggedIn(email);
            boardName = handleBoardName(boardName);
            //validateExists(emailAssigne);

            BoardBL targetBoard = null;
            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));
                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    targetBoard = boardEntry.Value;
                    break;
                }
            }

            if (targetBoard == null)
                ThrowAndLog("Board not found or user is not a member");

            targetBoard.AssigneTask(email, emailAssigne, taskId, selectedColumntype);

            _taskController.Update((int)taskId, targetBoard.BoardId, "AssigneeEmail", emailAssigne);
        }

        public LinkedList<TaskBL> GetInProgressTasks(string email)
        {
            validateLoggedIn(email);
            validateExists(email);

            LinkedList<TaskBL> allInProgressTasks = new LinkedList<TaskBL>();

            foreach (var userBoardsPair in boards)
            {
                foreach (var boardEntry in userBoardsPair.Value)
                {
                    BoardBL board = boardEntry.Value;
                    if (board.IsBoardMember(email))
                    {
                        LinkedList<TaskBL> inProgressTasksOnThisBoard = board.GetInProgressTasks(email);
                        foreach (var task in inProgressTasksOnThisBoard)
                        {
                            allInProgressTasks.AddLast(task);
                        }
                    }
                }
            }
            return allInProgressTasks;
        }

        public Dictionary<ColumnType, ColumnBL> GetAllColumns(string email, string boardName)
        {
            validateLoggedIn(email);
            boardName = handleBoardName(boardName);

            BoardBL targetBoard = null;
            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));
                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    targetBoard = boardEntry.Value;
                    break;
                }
            }

            if (targetBoard != null)
                return targetBoard.BoardColumns;

            ThrowAndLog("BoardName with that name doesnt exists or user is not a member");
            return null;
        }
        public List<int> GetUserBoards(string email)
        {
            validateLoggedIn(email);
            validateExists(email);

            List<int> boardIDs = new List<int>();

            foreach (var userBoardsPair in boards.Values)
            {
                foreach (var boardEntry in userBoardsPair)
                {
                    BoardBL board = boardEntry.Value;
                    if (board.IsBoardMember(email))
                        if (!boardIDs.Contains(board.BoardId))
                            boardIDs.Add(board.BoardId);
                }
            }
            return boardIDs;
        }

        public int GetColumnLimit(string email, string boardName, int columnNumber)
        {
            ColumnType selectedColumntype = validateColumnType(columnNumber);

            validateLoggedIn(email);

            boardName = handleBoardName(boardName);

            validateUserBoardListExists(email);

            validateBoardExists(email, boardName);

            return boards[email].FirstOrDefault(kvp => kvp.Key.Item1 == boardName).Value.GetColumnLimit(selectedColumntype);
        }

        public string GetColumnName(string email, string boardName, int columnNumber)
        {

            ColumnType selectedColumntype = validateColumnType(columnNumber);

            validateLoggedIn(email);

            boardName = handleBoardName(boardName);

            validateUserBoardListExists(email);

            validateBoardExists(email, boardName);

            return boards[email].FirstOrDefault(kvp => kvp.Key.Item1 == boardName).Value.GetColumnName(selectedColumntype);
        }

        public ColumnBL GetColumn(string email, string boardName, int columnNumber)
        {

            ColumnType selectedColumntype = validateColumnType(columnNumber);

            //validateLoggedIn(email);

            boardName = handleBoardName(boardName);

            validateUserBoardListExists(email);

            validateBoardExists(email, boardName);

            return boards[email].FirstOrDefault(kvp => kvp.Key.Item1 == boardName).Value.GetColumn(selectedColumntype);
        }

        public string GetBoardName(int boardId)
        {
            foreach (var userBoards in boards.Values)
            {
                var match = userBoards.FirstOrDefault(entry => entry.Key.Item2 == boardId);
                if (!match.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                    return match.Key.Item1;
            }
            ThrowAndLog("BoardName with that boardId doesnt exists");
            return null;
        }

        public Dictionary<string, List<int>> GetAllBoards(string email)
        {
            Dictionary<string, List<int>> boardsInfo = new Dictionary<string, List<int>>();

            foreach (var (userEmail, userBoards) in boards)
            {
                if(userEmail != email)
                {
                    var ids = new List<int>(userBoards.Count);
                    foreach (var board in userBoards.Values)
                        ids.Add(board.BoardId);

                    boardsInfo[userEmail] = ids;
                }      
            }
            return boardsInfo;
        }

        public List<string> GetAllBoardMembers(string email, string boardName)
        {
            boardName = handleBoardName(boardName);

            BoardBL targetBoard = null;
            foreach (var ownerBoards in boards.Values)
            {
                var boardEntry = ownerBoards.FirstOrDefault(kvp => kvp.Key.Item1 == boardName && kvp.Value.IsBoardMember(email));

                if (!boardEntry.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                {
                    targetBoard = boardEntry.Value;
                    break;
                }
            }

            if (targetBoard == null)
                ThrowAndLog("Board not found or user is not a member of it.");

            return new List<string>(targetBoard.BoardMembers.Keys);
        }

        public string GetBoardOwner(int boardId)
        {
            foreach (var userBoards in boards.Values)
            {
                var match = userBoards.FirstOrDefault(entry => entry.Key.Item2 == boardId);
                if (!match.Equals(default(KeyValuePair<(string, int), BoardBL>)))
                    return match.Value.boardDTO.OwnerEmail;
            }
            ThrowAndLog("Board with that boardId doesnt exists");
            return null;
        }

        public string LoadData()
        {
            boards.Clear();
            taskIdControl = 0;
            boardIDCounter = 0;

            var boardDtos = this._boardController.SelectAll();
            var columnDtos = this._columnController.SelectAll();
            var taskDtos = this._taskController.SelectAll();

            var loadedBoardsById = new Dictionary<long, BoardBL>();

            foreach (var boardDto in boardDtos)
            {
                boardDto.MarkAsPersisted();
                var bl = new BoardBL(boardDto.Name, (int)boardDto.Id)
                {
                    boardDTO = boardDto
                };
                bl.JoinBoard(boardDto.OwnerEmail);

                if (!boards.ContainsKey(boardDto.OwnerEmail))
                {
                    boards[boardDto.OwnerEmail] = new Dictionary<(string, int), BoardBL>();
                }

                boards[boardDto.OwnerEmail].Add((boardDto.Name, (int)boardDto.Id), bl);
                loadedBoardsById[boardDto.Id] = bl;
                boardIDCounter = Math.Max(boardIDCounter, (int)boardDto.Id);
            }

            foreach (var boardBl in loadedBoardsById.Values)
            {
                var members = _boardMembersController.GetBoardMembers(boardBl.BoardId);
                foreach (var memberEmail in members)
                {
                    try
                    {
                        boardBl.JoinBoard(memberEmail);
                    }
                    catch (Exception ex)
                    {
                    }
                   
                }
            }

            foreach (var columnDto in columnDtos)
            {
                columnDto.MarkAsPersisted();
                if (loadedBoardsById.TryGetValue(columnDto.BoardID, out BoardBL boardBl))
                {
                    var colType = (ColumnType)columnDto.ColumnOrdinal;
                    if (boardBl.BoardColumns.ContainsKey(colType))
                    {
                        boardBl.BoardColumns[colType].columnDTO = columnDto;
                        boardBl.BoardColumns[colType].TaskLimit = columnDto.TasksLimit;
                    }
                }
            }

            long maxTaskIdLoaded = -1;
            foreach (var taskDto in taskDtos)
            {
                taskDto.MarkAsPersisted();
                if (loadedBoardsById.TryGetValue(taskDto.BoardID, out BoardBL boardBl))
                {
                    var colType = (ColumnType)taskDto.ColumnOrdinal;
                    if (boardBl.BoardColumns.ContainsKey(colType))
                    {
                        var taskBl = new TaskBL(taskDto.TaskId, taskDto.CreationTime, taskDto.DueDate, taskDto.Title, taskDto.Description, taskDto)
                        {
                            Assignee = taskDto.Assignee,
                        };
                        if (!string.IsNullOrEmpty(taskDto.Assignee))
                        {
                            try
                            {
                                boardBl.JoinBoard(taskDto.Assignee);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        boardBl.BoardColumns[colType].AddTask(taskBl);
                        maxTaskIdLoaded = Math.Max(maxTaskIdLoaded, taskDto.TaskId);
                    }
                }
            }
            if (maxTaskIdLoaded > -1)
                taskIdControl = maxTaskIdLoaded + 1;

            return "success";
        }

        public string DeleteData()
        {
            _boardMembersController.DeleteAll();
            _taskController.DeleteAll();

            _columnController.DeleteAll();

            _boardController.DeleteAll();

            boards.Clear();
            taskIdControl = 0;
            boardIDCounter = 0;
            return "success";
        } 
    }
}
