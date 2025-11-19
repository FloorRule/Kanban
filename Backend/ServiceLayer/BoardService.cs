using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class BoardService
    {
        private BoardFacade boardFacade;

        internal BoardService(BoardFacade boardFacade)
        {
            this.boardFacade = boardFacade;
        }

        /// <summary>
        /// Creates a new board with the given name for the user.
        /// </summary>
        /// <param name="email">The email of the user creating the board.</param>
        /// <param name="name">The name of the board to be created.</param>
        /// <returns>A JSON-formatted string indicating success or an error message.</returns>
        /// <remarks>
        /// Preconditions:
        /// - The email must belong to a logged-in user.
        /// - The board name must be non-empty and unique for that user.
        /// 
        /// Postconditions:
        /// - A new board is added to the user's board list if validation passes.
        /// </remarks>
        public string CreateBoard(string email, string name)
        {
            Response res;
            try
            {
                this.boardFacade.CreateBoard(email, name);
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
        /// Deletes a board that belongs to the specified user.
        /// </summary>
        /// <param name="email">The email of the board owner.</param>
        /// <param name="boardName">The name of the board to delete.</param>
        /// <returns>A JSON-formatted string indicating success or an error message.</returns>
        /// <remarks>
        /// Preconditions:
        /// - The user must be logged in and own the specified board.
        /// 
        /// Postconditions:
        /// - The specified board is removed from the user's list of boards.
        /// </remarks>
        public string DeleteBoard(string email, string boardName)
        {
            Response res;
            try
            {
                this.boardFacade.DeleteBoard(email, boardName);
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
        /// Sets a task limit on a specific column in a user's board.
        /// </summary>
        /// <param name="email">The email of the board owner.</param>
        /// <param name="boardName">The name of the board containing the column.</param>
        /// <param name="columnNumber">The column to apply the task limit to.</param>
        /// <param name="limit">The new task limit to enforce.</param>
        /// <returns>A JSON-formatted string indicating success or an error message.</returns>
        /// <remarks>
        /// Preconditions:
        /// - The user must be logged in.
        /// - The board and column must exist.
        /// - The limit must be positive and not less than the current number of tasks in the column.
        /// 
        /// Postconditions:
        /// - The column's task limit is updated if all checks pass.
        /// </remarks>
        public string LimitColumn(string email, string boardName, int columnNumber, int limit)
        {
            Response res;
            try
            {
                this.boardFacade.LimitColumn(email, boardName, columnNumber, limit);
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
        /// Transfers ownership of a board from one user to another.
        /// 
        /// Preconditions: The current owner must own the board, and the new owner must be a member of the board.
        /// Postconditions: The board ownership is transferred. Returns a confirmation or error message.
        /// </summary>
        /// <param name="currentOwnerEnail">Email of the current board owner.</param>
        /// <param name="newOwnerEnail">Email of the new board owner.</param>
        /// <param name="boardName">The name of the board whose ownership is being transferred.</param>
        /// <returns>A success or error message.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string TransferOwnerShip(string currentOwnerEnail, string newOwnerEnail, string boardName)
        {
            Response res;
            try
            {

                boardFacade.TransferOwnerShip(currentOwnerEnail, newOwnerEnail, boardName);
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
        /// Allows a user to join an existing board.
        /// 
        /// Preconditions: The board must exist and the user must not already be a member.
        /// Postconditions: The user becomes a member of the board. Returns a confirmation or error message.
        /// </summary>
        /// <param name="email">Email of the user requesting to join the board.</param>
        /// <param name="boardID">The ID of the board to join.</param>
        /// <returns>A success or error message.</returns>
        /// <exception cref="NotImplementedException"></exception>

        public string JoinBoard(string email, int boardID)
        {
            Response res;
            try
            {

                boardFacade.JoinBoard(email, boardID);
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
        /// Allows a user to leave a board they are currently a member of.
        /// 
        /// Preconditions: The user must be a member of the board. The user must not be the only owner if ownership transfer is required.
        /// Postconditions: The user is removed from the board. Returns a confirmation or error message.
        /// </summary>
        /// <param name="email">Email of the user requesting to leave the board.</param>
        /// <param name="boardID">The ID of the board to leave.</param>
        /// <returns>A success or error message.</returns>
        /// <exception cref="NotImplementedException"></exception>

        public string LeaveBoard(string email, int boardID)
        {
            Response res;
            try
            {

                boardFacade.LeaveBoard(email, boardID);
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
        /// Retrieves all columns for a specific board belonging to the user.
        /// </summary>
        /// <param name="email">The email of the board owner.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <returns>A JSON-formatted string containing column data or an error message.</returns>
        /// <remarks>
        /// Preconditions:
        /// - The user must be logged in.
        /// - The board must exist and belong to the user.
        /// 
        /// Postconditions:
        /// - Returns a mapping of column types to column data, or an error if not found.
        /// </remarks>
        public string GetAllColumns(string email, string boardName)
        {
            Response res;
            try
            {
                Dictionary<ColumnType, ColumnBL> cols = boardFacade.GetAllColumns(email, boardName);
                res = new Response(Convertor.columnConverterSL(cols));
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        /// <summary>
        /// Retrieves all boards that belong to a specific user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>A JSON-formatted string containing the user's boards or an error message.</returns>
        /// <remarks>
        /// Preconditions:
        /// - The user must be logged in and exist in the system.
        /// 
        /// Postconditions:
        /// - Returns a list of boards associated with the user, or an error if none exist.
        /// </remarks>
        public string GetUserBoards(string email)
        {
            Response res;
            try
            {
                List<int> userBoardID = boardFacade.GetUserBoards(email);
                res = new Response(userBoardID);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string GetColumnLimit(string email, string boardName, int columnNumber)
        {
            Response res;
            try
            {
                int limit = boardFacade.GetColumnLimit(email, boardName, columnNumber);
                res = new Response(limit);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string GetColumnName(string email, string boardName, int columnNumber)
        {
            Response res;
            try
            {
                string columnName = boardFacade.GetColumnName(email, boardName, columnNumber);
                res = new Response(columnName);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string GetColumn(string email, string boardName, int columnNumber)
        {
            Response res;
            try
            {
                LinkedList<TaskBL> tasksBL = boardFacade.GetColumn(email, boardName, columnNumber).Tasks;
                res = new Response(Convertor.taskConverterSL(tasksBL));
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        /// <summary>		 
        /// This method returns a board's name		 
        /// </summary>		 
        /// <param name="boardId">The board's ID</param>		 
        /// <returns>A response with the board's name, unless an error occurs (see <see cref="GradingService"/>)</returns>		 
        public string GetBoardName(int boardId)
        {
            Response res;
            try
            {
                string boardName = boardFacade.GetBoardName(boardId);
                res = new Response(boardName);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string GetAllBoards(string email)
        {
            Response res;
            try
            {
                Dictionary<string, List<int>> usersBoardID = boardFacade.GetAllBoards(email);
                res = new Response(usersBoardID);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string GetAllBoardMembers(string email, string boardName)
        {
            Response res;
            try
            {
                List<string> boardMembers = boardFacade.GetAllBoardMembers(email, boardName);
                res = new Response(boardMembers);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string GetBoardOwner(int boardId)
        {
            Response res;
            try
            {
                string ownerEmail = boardFacade.GetBoardOwner(boardId);
                res = new Response(ownerEmail);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string LoadData()
        {
            Response res;
            try
            {
                boardFacade.LoadData();
                res = new Response();
                return JsonSerializer.Serialize(res);
            }
            catch (Exception ex)
            {
                res = new Response(ex.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        public string DeleteData()
        {
            Response res;
            try
            {
                boardFacade.DeleteData();
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
