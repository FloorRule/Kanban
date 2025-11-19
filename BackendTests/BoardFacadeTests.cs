using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.ServiceLayer;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json;

namespace BackendTests
{
    [TestFixture]
    public class BoardFacadeTests
    {
        private BoardFacade facade;
        private AuthenticationFacade auth;
        private readonly string email = "test.owner@example.com";
        private readonly string secondUser = "test.member@example.com";
        private readonly string thirdUser = "test.outsider@example.com";
        private readonly string fourthUser = "test.anothermember@example.com";

        [SetUp]
        public void Setup()
        {
            TestDbBoot.EnsureSchemaExists();

            auth = new AuthenticationFacade();
            facade = new BoardFacade(auth);

            // Clear all data before each test to make sure we start fresh
            facade.DeleteData();
            auth.Clear();

            // Register users and log in for testing purposes
            auth.Register(email);
            auth.Login(email);

            auth.Register(secondUser);
            auth.Login(secondUser);

            auth.Register(thirdUser);

            auth.Register(fourthUser);
            auth.Login(fourthUser);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up everything after each test to avoid weird behavior
            facade.DeleteData();
            new UserFacade(auth).DeleteData();
            auth.Clear();
        }

        #region Create/Delete Board Tests

        [Test]
        public void CreateBoard_Valid_DoesNotThrowAndInitializesCorrectly()
        {
            // Should create a board without errors and set it up correctly
            Assert.DoesNotThrow(() => facade.CreateBoard(email, "My New Board"));
            var boards = facade.GetUserBoards(email);
            Assert.AreEqual(1, boards.Count);
            var boardName = facade.GetBoardName(boards.First());
            Assert.AreEqual("My New Board", boardName);
            var columns = facade.GetAllColumns(email, "My New Board");
            Assert.That(columns.Values.All(c => c.TaskLimit == -1));
        }

        [Test]
        public void CreateBoard_NameWithWhitespace_TrimsName()
        {
            // Should remove leading/trailing spaces from board name
            facade.CreateBoard(email, "  Spaced Board  ");
            var boardId = facade.GetUserBoards(email).First();
            Assert.AreEqual("Spaced Board", facade.GetBoardName(boardId));
        }

        [Test]
        public void DeleteBoard_ValidOwner_DeletesSuccessfully()
        {
            // Owner deletes their board and it should be completely gone
            facade.CreateBoard(email, "ToDelete");
            var boardId = facade.GetUserBoards(email).First();
            Assert.DoesNotThrow(() => facade.DeleteBoard(email, "ToDelete"));
            Assert.IsEmpty(facade.GetUserBoards(email));
            var ex = Assert.Throws<Exception>(() => facade.GetBoardName(boardId));
            Assert.That(ex.Message, Is.EqualTo("BoardName with that boardId doesnt exists"));
        }

        [Test]
        public void DeleteBoard_NotOwner_Throws()
        {
            // Someone else trying to delete the board should get an error
            facade.CreateBoard(email, "SharedBoard");
            var ex = Assert.Throws<Exception>(() => facade.DeleteBoard(secondUser, "SharedBoard"));
            Assert.That(ex.Message, Is.EqualTo("User is not on board list"));
        }

        #endregion

        #region Member Authorization & Lookup Tests

        [Test]
        public void LimitColumn_ActionByMemberFails()
        {
            // Regular members shouldn't be allowed to limit columns
            facade.CreateBoard(email, "SharedBoard");
            var boardId = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, boardId);
            var ex = Assert.Throws<Exception>(() => facade.LimitColumn(secondUser, "SharedBoard", 0, 10));
            Assert.That(ex.Message, Is.EqualTo("User is not on board list"));
        }

        [Test]
        public void Fixed_MoveTask_ActionByAssigneeMemberSucceeds()
        {
            // If a member is assigned a task, they should be able to move it
            facade.CreateBoard(email, "SharedBoard");
            var boardId = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, boardId);
            var taskId = facade.AddTask(email, "SharedBoard", "Task", "D", DateTime.Now.AddDays(1));
            facade.AssigneTask(email, "SharedBoard", secondUser, taskId, 0);
            Assert.DoesNotThrow(() => facade.MoveTaskForwards(secondUser, "SharedBoard", taskId));
            var columns = facade.GetAllColumns(email, "SharedBoard");
            Assert.IsEmpty(columns[ColumnType.BackLog].Tasks);
            Assert.IsNotEmpty(columns[ColumnType.InProgress].Tasks);
            Assert.That(columns[ColumnType.InProgress].Tasks.First().taskId, Is.EqualTo(taskId));
        }

        [Test]
        public void Fixed_UpdateTask_ActionByAssigneeMemberSucceeds()
        {
            // Assignee should be able to update their task title
            facade.CreateBoard(email, "SharedBoard");
            var boardId = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, boardId);
            var taskId = facade.AddTask(email, "SharedBoard", "Task", "D", DateTime.Now.AddDays(1));
            facade.AssigneTask(email, "SharedBoard", secondUser, taskId, 0);
            Assert.DoesNotThrow(() => facade.UpdateTask(secondUser, "SharedBoard", 0, (int)taskId, SystemAction.UpdateTitle, "New Title"));
            var columns = facade.GetAllColumns(email, "SharedBoard");
            var updatedTask = columns[ColumnType.BackLog].Tasks.First();
            Assert.AreEqual("New Title", updatedTask.Title);
        }

        [Test]
        public void RemoveTask_ByMember_SucceedsBecauseOfCorrectLookup()
        {
            // Member should be able to remove task from a board they’re part of
            facade.CreateBoard(email, "SharedBoard");
            var boardId = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, boardId);
            var taskId = facade.AddTask(email, "SharedBoard", "Task", "D", DateTime.Now.AddDays(1));
            Assert.DoesNotThrow(() => facade.RemoveTask(secondUser, "SharedBoard", taskId));
        }

        #endregion

        #region Stress and Complex Interaction Tests

        [Test]
        public void MultipleBoards_ActionOnOneDoesNotAffectOther()
        {
            // Adding/removing tasks in one board shouldn’t mess with the other
            facade.CreateBoard(email, "BoardA");
            facade.CreateBoard(email, "BoardB");
            var taskA_id = facade.AddTask(email, "BoardA", "Task A", "d", DateTime.Now.AddDays(1));
            facade.AddTask(email, "BoardB", "Task B", "d", DateTime.Now.AddDays(1));
            facade.RemoveTask(email, "BoardA", taskA_id);
            var columnsA = facade.GetAllColumns(email, "BoardA");
            Assert.IsEmpty(columnsA[ColumnType.BackLog].Tasks);
            var columnsB = facade.GetAllColumns(email, "BoardB");
            Assert.IsNotEmpty(columnsB[ColumnType.BackLog].Tasks);
            Assert.That(columnsB[ColumnType.BackLog].Tasks.First().Title, Is.EqualTo("Task B"));
        }

        [Test]
        public void ComplexOwnershipTransferChain_StateRemainsConsistent()
        {
            // Ownership goes from A → B → C, only final owner should have access
            facade.CreateBoard(email, "HotPotato");
            var boardId = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, boardId);
            auth.Login(thirdUser);
            facade.JoinBoard(thirdUser, boardId);
            facade.TransferOwnerShip(email, secondUser, "HotPotato");
            facade.TransferOwnerShip(secondUser, thirdUser, "HotPotato");
            Assert.DoesNotThrow(() => facade.LimitColumn(thirdUser, "HotPotato", 0, 5));
            var ex1 = Assert.Throws<Exception>(() => facade.LimitColumn(email, "HotPotato", 0, 5));
            Assert.That(ex1.Message, Is.EqualTo("User is not on board list"));
            var ex2 = Assert.Throws<Exception>(() => facade.LimitColumn(secondUser, "HotPotato", 0, 5));
            Assert.That(ex2.Message, Is.EqualTo("User is not on board list"));
        }

        [Test]
        public void JoinLeaveJoinCycle_HandledCorrectly()
        {
            // User should be able to leave and rejoin the same board
            facade.CreateBoard(email, "Project");
            var boardId = facade.GetUserBoards(email)[0];
            Assert.DoesNotThrow(() => facade.JoinBoard(secondUser, boardId));
            Assert.DoesNotThrow(() => facade.LeaveBoard(secondUser, boardId));
            Assert.DoesNotThrow(() => facade.JoinBoard(secondUser, boardId));
        }

        [Test]
        public void AddTaskWithMaxLentghValues_DoesNotThrow()
        {
            // Should allow tasks with max-length title and description
            facade.CreateBoard(email, "LongContent");
            string maxTitle = new string('a', TaskBL.MaxTitleLength);
            string maxDesc = new string('b', TaskBL.MaxDescriptionLength);
            Assert.DoesNotThrow(() => facade.AddTask(email, "LongContent", maxTitle, maxDesc, DateTime.Now.AddDays(1)));
            var columns = facade.GetAllColumns(email, "LongContent");
            var task = columns[ColumnType.BackLog].Tasks.First();
            Assert.AreEqual(maxTitle, task.Title);
            Assert.AreEqual(maxDesc, task.Description);
        }

        [Test]
        public void SimulatedConcurrency_DeleteBoardWhileMemberActs_ActionFails()
        {
            // Simulate member trying to act on a deleted board (should fail)
            facade.CreateBoard(email, "RaceConditionBoard");
            var boardId = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, boardId);
            facade.DeleteBoard(email, "RaceConditionBoard");
            var ex = Assert.Throws<Exception>(() => facade.MoveTaskForwards(secondUser, "RaceConditionBoard", 0));
            Assert.That(ex.Message, Is.EqualTo("Board with that name is not found or User is not a member"));
        }

        [Test]
        public void GetInProgressTasks_UserIsMemberOfNoBoards_ReturnsEmptyList()
        {
            // Should return an empty list if user isn’t part of any boards
            auth.Login(thirdUser);
            auth.Logout(thirdUser);
            auth.Login(thirdUser);
            var tasks = facade.GetInProgressTasks(thirdUser);
            Assert.IsNotNull(tasks);
            Assert.IsEmpty(tasks);
        }

        #endregion
        #region Data Persistence Tests

        [Test]
        public void Full_LoadData_AfterComplexSetup_RestoresStateExhaustively()
        {
            // Trying to build a messy but real-life example and then reload it
            var dueDate = DateTime.Now.AddDays(10);
            var boardName1 = "Alpha Project";
            var boardName2 = "Side Hustle";

            // --- Board 1: Created by main user, shared with secondUser ---
            facade.CreateBoard(email, boardName1);
            var board1Id = facade.GetUserBoards(email).First();
            facade.JoinBoard(secondUser, board1Id);
            facade.LimitColumn(email, boardName1, (int)ColumnType.InProgress, 3);

            var task1 = facade.AddTask(email, boardName1, "Task 1: In Backlog", "Desc1", dueDate); // Will stay in backlog
            var task2 = facade.AddTask(email, boardName1, "Task 2: To Progress", "Desc2", dueDate);
            var task3 = facade.AddTask(email, boardName1, "Task 3: To Done", "Desc3", dueDate);

            // Moving and assigning tasks for fun
            facade.AssigneTask(email, boardName1, secondUser, task2, 0);
            facade.MoveTaskForwards(secondUser, boardName1, task2);

            facade.AssigneTask(email, boardName1, email, task3, 0);
            facade.MoveTaskForwards(email, boardName1, task3);
            facade.MoveTaskForwards(email, boardName1, task3);

            // --- Board 2: Created by second user ---
            facade.CreateBoard(secondUser, boardName2);
            var task4 = facade.AddTask(secondUser, boardName2, "Side Task", "Side Desc", dueDate);

            // Now simulate restarting the app/backend
            var newAuth = new AuthenticationFacade();
            var newFacade = new BoardFacade(newAuth);
            newAuth.Register(email);
            newAuth.Register(secondUser);
            newAuth.Register(thirdUser);
            newAuth.Register(fourthUser);

            Assert.DoesNotThrow(() => newFacade.LoadData());

            newAuth.Login(email);
            newAuth.Login(secondUser);

            // Double check if everything came back the way we left it
            var allUser1Boards = newFacade.GetUserBoards(email);
            var b1Id_loaded = allUser1Boards.First(id => newFacade.GetBoardName(id) == boardName1);
            Assert.AreEqual(boardName1, newFacade.GetBoardName(b1Id_loaded));
            Assert.AreEqual(3, newFacade.GetColumnLimit(email, boardName1, (int)ColumnType.InProgress));

            var b1_cols = newFacade.GetAllColumns(email, boardName1);
            Assert.AreEqual(1, b1_cols[ColumnType.BackLog].Tasks.Count);
            Assert.AreEqual(1, b1_cols[ColumnType.InProgress].Tasks.Count);
            Assert.AreEqual(1, b1_cols[ColumnType.Done].Tasks.Count);

            // Making sure task info was loaded correctly too
            Assert.AreEqual("Task 1: In Backlog", b1_cols[ColumnType.BackLog].Tasks.First().Title);
            Assert.AreEqual("", b1_cols[ColumnType.BackLog].Tasks.First().Assignee);

            Assert.AreEqual(secondUser, b1_cols[ColumnType.InProgress].Tasks.First().Assignee);
            Assert.AreEqual(email, b1_cols[ColumnType.Done].Tasks.First().Assignee);

            // Board 2 also needs to still be there
            var allUser2Boards = newFacade.GetUserBoards(secondUser);
            var b2Id_loaded = allUser2Boards.First(id => newFacade.GetBoardName(id) == boardName2);
            var b2_cols = newFacade.GetAllColumns(secondUser, boardName2);
            Assert.AreEqual(1, b2_cols[ColumnType.BackLog].Tasks.Count);
            Assert.AreEqual("Side Task", b2_cols[ColumnType.BackLog].Tasks.First().Title);
        }

        [Test]
        public void DeleteData_ClearsAllInformation()
        {
            // After calling DeleteData, nothing should survive
            facade.CreateBoard(email, "Board1");
            facade.AddTask(email, "Board1", "Task", "d", DateTime.Now.AddDays(1));

            Assert.DoesNotThrow(() => facade.DeleteData());

            Assert.IsEmpty(facade.GetUserBoards(email));

            // Simulate a restart to make sure even the DB is cleared
            var newAuth = new AuthenticationFacade();
            var newFacade = new BoardFacade(newAuth);
            Assert.DoesNotThrow(() => newFacade.LoadData());
            newAuth.Register(email);
            newAuth.Login(email);
            Assert.IsEmpty(newFacade.GetUserBoards(email));
        }

        #endregion

        [Test]
        public void GetBoardName_Survives_Full_MultiUser_Lifecycle_And_Persistence()
        {
            // End-to-end test: make sure board name is always retrievable
            int boardId = -1;
            string boardName = "project phoenix";

            // === Step 1: Create the board ===
            Assert.DoesNotThrow(() => facade.CreateBoard(email, boardName));
            boardId = facade.GetUserBoards(email).First();

            // Should return the name right after creation
            Assert.AreEqual(boardName, facade.GetBoardName(boardId));

            // === Step 2: Add some members ===
            Assert.DoesNotThrow(() => facade.JoinBoard(secondUser, boardId));
            Assert.AreEqual(boardName, facade.GetBoardName(boardId));

            // === Step 3: Add tasks, assign, move ===
            var task1Id = facade.AddTask(email, boardName, "Task 1", "Desc 1", DateTime.Now.AddDays(5));
            var task2Id = facade.AddTask(secondUser, boardName, "Task 2", "Desc 2", DateTime.Now.AddDays(10));

            facade.AssigneTask(email, boardName, secondUser, task1Id, 0);
            facade.MoveTaskForwards(secondUser, boardName, task1Id);
            Assert.AreEqual(boardName, facade.GetBoardName(boardId));

            // === Step 4: Transfer ownership to another member ===
            Assert.DoesNotThrow(() => facade.JoinBoard(fourthUser, boardId));
            facade.TransferOwnerShip(email, fourthUser, boardName);
            Assert.AreEqual(boardName, facade.GetBoardName(boardId));

            // === Step 5: Simulate reboot and check again ===
            var newAuth = new AuthenticationFacade();
            var newFacade = new BoardFacade(newAuth);
            newFacade.LoadData();
            newAuth.Register(email);
            newAuth.Register(secondUser);
            newAuth.Register(fourthUser);
            newAuth.Login(email);
            newAuth.Login(secondUser);
            newAuth.Login(fourthUser);

            // Board name should still be retrievable after everything
            Assert.AreEqual(boardName, newFacade.GetBoardName(boardId));
        }

        #region Standard Persistence Test (Now with Member Verification)

        [Test]
        public void Full_LoadData_AfterComplexSetup2_RestoresStateExhaustively()
        {
            // This one also checks if members with no tasks are remembered
            var dueDate = DateTime.Now.AddDays(10);
            var boardName1 = "Alpha Project";
            var boardName2 = "Side Hustle";

            facade.CreateBoard(email, boardName1);
            var board1Id = facade.GetUserBoards(email).First();

            // secondUser joins but never gets a task
            facade.JoinBoard(secondUser, board1Id);

            // fourthUser joins and gets a task
            facade.JoinBoard(fourthUser, board1Id);

            facade.LimitColumn(email, boardName1, (int)ColumnType.InProgress, 3);

            var task1 = facade.AddTask(email, boardName1, "Task 1: In Backlog", "Desc1", dueDate);
            var task2 = facade.AddTask(email, boardName1, "Task 2: To Progress", "Desc2", dueDate);
            var task3 = facade.AddTask(email, boardName1, "Task 3: To Done", "Desc3", dueDate);

            facade.AssigneTask(email, boardName1, fourthUser, task2, 0);
            facade.MoveTaskForwards(fourthUser, boardName1, task2);

            facade.AssigneTask(email, boardName1, email, task3, 0);
            facade.MoveTaskForwards(email, boardName1, task3);
            facade.MoveTaskForwards(email, boardName1, task3);

            // Make another board owned by secondUser
            facade.CreateBoard(secondUser, boardName2);
            int board2Id = -1;
            foreach (var item in facade.GetUserBoards(secondUser))
            {
                if (boardName2 == facade.GetBoardName(item))
                    board2Id = item;
            }
            Assert.IsTrue(board2Id != -1);
            facade.AddTask(secondUser, boardName2, "Side Task", "Side Desc", dueDate);

            // Simulate a full restart
            var newAuth = new AuthenticationFacade();
            var newFacade = new BoardFacade(newAuth);

            Assert.DoesNotThrow(() => newFacade.LoadData());

            newAuth.Register(email);
            newAuth.Register(secondUser);
            newAuth.Register(fourthUser);

            newAuth.Login(email);
            newAuth.Login(secondUser);
            newAuth.Login(fourthUser);

            // Check if members and tasks are all properly back
            Assert.AreEqual(boardName1, newFacade.GetBoardName(board1Id));
            Assert.AreEqual(3, newFacade.GetColumnLimit(email, boardName1, (int)ColumnType.InProgress));

            var b1_cols = newFacade.GetAllColumns(email, boardName1);
            Assert.AreEqual(1, b1_cols[ColumnType.BackLog].Tasks.Count);
            Assert.AreEqual(1, b1_cols[ColumnType.InProgress].Tasks.Count);
            Assert.AreEqual(1, b1_cols[ColumnType.Done].Tasks.Count);
            Assert.AreEqual(fourthUser, b1_cols[ColumnType.InProgress].Tasks.First().Assignee);

            var b2_cols = newFacade.GetAllColumns(secondUser, boardName2);
            Assert.AreEqual(1, b2_cols[ColumnType.BackLog].Tasks.Count);
        }

        #endregion
    }
}


