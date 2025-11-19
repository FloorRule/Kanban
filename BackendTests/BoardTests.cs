
using IntroSE.Kanban.Backend.ServiceLayer;
using System.Text.Json;


namespace BackendTests
{
    public class BoardTests
    {
        private readonly BoardService boardService;
        private readonly UserService userService;

        public BoardTests(BoardService boardService, UserService userService)
        {
            this.boardService = boardService;
            this.userService = userService;
        }

        /// <summary>
        /// Runs a sequence of tests demonstrating the board's lifecycle.
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("------ Board Tests ------");

            // Setup: Create and LOG OUT users required for the tests for a clean state
            userService.Register("owner@example.com", "Password123");
            userService.Logout("owner@example.com"); // Logout after register
            userService.Register("member@example.com", "Password123");
            userService.Logout("member@example.com");
            userService.Register("newowner@example.com", "Password123");
            userService.Logout("newowner@example.com");

            // Now login the users you need to be active
            userService.Login("owner@example.com", "Password123");
            userService.Login("member@example.com", "Password123");
            userService.Login("newowner@example.com", "Password123");

            // --- Board Creation ---
            Console.WriteLine("\n--- Testing Board Creation ---");
            TestBoardCreation("owner@example.com", "Project Phoenix", expectSuccess: true);
            TestBoardCreation("owner@example.com", "", expectSuccess: false); // Invalid name
            TestBoardCreation("nouser@example.com", "some board", expectSuccess: false); // Non-existent user

            // Get the ID of the created board for subsequent tests
            int boardId = GetFirstBoardId("owner@example.com");
            if (boardId == -1)
            {
                Console.WriteLine("Could not retrieve board ID. Aborting further tests.");
                return;
            }

            // --- Board and Column Info ---
            Console.WriteLine("\n--- Testing Board and Column Info ---");
            TestGetBoardName(boardId, "Project Phoenix", expectSuccess: true);
            TestGetBoardName(999, "not important", expectSuccess: false); // Invalid ID

            TestLimitColumn("owner@example.com", "Project Phoenix", 0, 5, expectSuccess: true);
            TestGetColumnLimit("owner@example.com", "Project Phoenix", 0, 5, expectSuccess: true);
            TestGetColumnName("owner@example.com", "Project Phoenix", 0, "backlog", expectSuccess: true);

            // --- Member Management ---
            Console.WriteLine("\n--- Testing Member Management ---");
            TestJoinBoard("member@example.com", boardId, expectSuccess: true);
            TestJoinBoard("member@example.com", boardId, expectSuccess: false); // Already a member
            TestLeaveBoard("member@example.com", boardId, expectSuccess: true);
            TestLeaveBoard("member@example.com", boardId, expectSuccess: false); // No longer a member

            // --- Ownership Transfer ---
            Console.WriteLine("\n--- Testing Ownership Transfer ---");
            TestTransferOwnership("owner@example.com", "newowner@example.com", "Project Phoenix", expectSuccess: false); // New owner not a member
            TestJoinBoard("newowner@example.com", boardId, expectSuccess: true); // New owner joins
            TestTransferOwnership("owner@example.com", "newowner@example.com", "Project Phoenix", expectSuccess: true);

            // Verify new owner has rights, and old owner does not
            TestLimitColumn("newowner@example.com", "Project Phoenix", 1, 10, expectSuccess: true);
            TestLimitColumn("owner@example.com", "Project Phoenix", 1, 10, expectSuccess: false); // Old owner is now just a member

            // --- Board Deletion ---
            Console.WriteLine("\n--- Testing Board Deletion ---");
            TestBoardDeletion("owner@example.com", "Project Phoenix", expectSuccess: false); // Not owner anymore
            TestBoardDeletion("newowner@example.com", "Project Phoenix", expectSuccess: true);
        }

        #region Individual Test Methods

        private void TestBoardCreation(string email, string boardName, bool expectSuccess)
        {
            string label = expectSuccess ? $"Create board '{boardName}' (Success)" : $"Create board '{boardName}' (Failure)";
            PrintResult(label, boardService.CreateBoard(email, boardName));
        }

        private void TestBoardDeletion(string email, string boardName, bool expectSuccess)
        {
            string label = expectSuccess ? $"Delete board '{boardName}' (Success)" : $"Delete board '{boardName}' (Failure)";
            PrintResult(label, boardService.DeleteBoard(email, boardName));
        }

        private void TestLimitColumn(string email, string boardName, int col, int limit, bool expectSuccess)
        {
            string label = expectSuccess ? $"Limit column {col} on '{boardName}' (Success)" : $"Limit column {col} on '{boardName}' (Failure)";
            PrintResult(label, boardService.LimitColumn(email, boardName, col, limit));
        }

        private void TestGetColumnLimit(string email, string boardName, int col, int expectedLimit, bool expectSuccess)
        {
            string label = expectSuccess ? $"Get limit for column {col} (Success)" : $"Get limit for column {col} (Failure)";
            PrintResult(label, boardService.GetColumnLimit(email, boardName, col));
        }

        private void TestGetColumnName(string email, string boardName, int col, string expectedName, bool expectSuccess)
        {
            string label = expectSuccess ? $"Get name for column {col} (Success)" : $"Get name for column {col} (Failure)";
            PrintResult(label, boardService.GetColumnName(email, boardName, col));
        }

        private void TestGetBoardName(int boardId, string expectedName, bool expectSuccess)
        {
            string label = expectSuccess ? $"Get name for board ID {boardId} (Success)" : $"Get name for board ID {boardId} (Failure)";
            PrintResult(label, boardService.GetBoardName(boardId));
        }

        private void TestJoinBoard(string email, int boardId, bool expectSuccess)
        {
            string label = expectSuccess ? $"User '{email}' joins board {boardId} (Success)" : $"User '{email}' joins board {boardId} (Failure)";
            PrintResult(label, boardService.JoinBoard(email, boardId));
        }

        private void TestLeaveBoard(string email, int boardId, bool expectSuccess)
        {
            string label = expectSuccess ? $"User '{email}' leaves board {boardId} (Success)" : $"User '{email}' leaves board {boardId} (Failure)";
            PrintResult(label, boardService.LeaveBoard(email, boardId));
        }

        private void TestTransferOwnership(string currentOwner, string newOwner, string boardName, bool expectSuccess)
        {
            string label = expectSuccess ? "Transfer ownership (Success)" : "Transfer ownership (Failure)";
            PrintResult(label, boardService.TransferOwnerShip(currentOwner, newOwner, boardName));
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

        private int GetFirstBoardId(string email)
        {
            string jsonResponse = boardService.GetUserBoards(email);
            Response? response = JsonSerializer.Deserialize<Response>(jsonResponse);
            if (!response.ErrorOccurred && response.ReturnValue != null)
            {
                JsonElement returnValue = (JsonElement)response.ReturnValue;
                List<int> boardIds = JsonSerializer.Deserialize<List<int>>(returnValue.GetRawText());
                return boardIds.FirstOrDefault(-1);
            }
            return -1;
        }

        #endregion
    }
}