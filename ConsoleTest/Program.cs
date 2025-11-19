using IntroSE.Kanban.Backend.ServiceLayer;
using IntroSE.Kanban.Backend.DataAccessLayer;
using BackendTests;
using System;

namespace Testing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Ensure the database schema is up-to-date before running tests.
            TestDbBoot.EnsureSchemaExists();

            // Create a single FactoryService to manage all underlying services.
            var factory = new FactoryService();

            // Start with a clean slate for every full test run.
            // This ensures tests are isolated and repeatable.
            factory.DeleteData();

            // Instantiate the test runner classes, passing the required services.
            var userTester = new UserTest(factory);
            var boardTester = new BoardTests(factory.boardService, factory.userService);
            var tasksTester = new TasksTest(factory);

            // Execute the comprehensive test suites from each class.

            Console.WriteLine("\n=========================");
            Console.WriteLine("   RUNNING USER TESTS");
            Console.WriteLine("=========================");
            userTester.RunAllTests();

            Console.WriteLine("\n=========================");
            Console.WriteLine("   RUNNING BOARD TESTS");
            Console.WriteLine("=========================");
            boardTester.RunAllTests();

            Console.WriteLine("\n=========================");
            Console.WriteLine("   RUNNING TASK TESTS");
            Console.WriteLine("=========================");
            tasksTester.RunAllTests();

            Console.WriteLine("\n\nAll automated tests completed.");
        }
    }
}