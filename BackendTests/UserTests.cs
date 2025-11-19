﻿
using System.Text.Json;
using IntroSE.Kanban.Backend.ServiceLayer;
using System;

namespace BackendTests
{
    public class UserTest
    {
        private readonly UserService userService;
        private readonly FactoryService factory; // For DeleteData

        public UserTest(FactoryService factoryService)
        {
            this.userService = factoryService.userService;
            this.factory = factoryService;
        }

        public void RunAllTests()
        {
            Console.WriteLine("------ User and System Tests ------");

            //--- User Registration and Login ---
            Console.WriteLine("\n--- Testing Registration and Login ---");
            TestRegister("valid@email.com", "Password123", expectSuccess: true);
            TestRegister("valid@email.com", "Password123", expectSuccess: false); // Duplicate
            TestLogin("valid@email.com", "Password123", expectSuccess: true);
            TestLogin("valid@email.com", "WrongPassword", expectSuccess: false);
            TestLogout("valid@email.com", expectSuccess: true);
            TestLogout("valid@email.com", expectSuccess: false); // Already logged out

            //--- Validation Tests ---
            Console.WriteLine("\n--- Testing Password Validations ---");
            RunValidationTests();

            //--- Data Persistence ---
            Console.WriteLine("\n--- Testing Data Persistence ---");
            TestDeleteData();
            TestRegister("persistent@user.com", "Password123", true);
            TestLoadData(); // Load the persistent user
            TestLogin("persistent@user.com", "Password123", true); // Should succeed after load
            TestDeleteData(); // Clean up
        }


        #region Individual Test Methods

        public void TestRegister(string email, string password, bool expectSuccess)
        {
            string label = expectSuccess ? $"Register '{email}' (Success)" : $"Register '{email}' (Failure)";
            PrintResult(label, userService.Register(email, password));
        }

        public void TestLogin(string email, string password, bool expectSuccess)
        {
            string label = expectSuccess ? $"Login '{email}' (Success)" : $"Login '{email}' (Failure)";
            PrintResult(label, userService.Login(email, password));
        }

        public void TestLogout(string email, bool expectSuccess)
        {
            string label = expectSuccess ? $"Logout '{email}' (Success)" : $"Logout '{email}' (Failure)";
            PrintResult(label, userService.Logout(email));
        }

        public void TestDeleteData()
        {
            PrintResult("Delete all data", factory.DeleteData());
        }

        public void TestLoadData()
        {
            PrintResult("Load all data", factory.LoadData());
        }

        public void RunValidationTests()
        {
            Console.WriteLine("Testing invalid passwords...");
            string[] badPasswords = {
                "abc", "123456", "ABCDEF", "abc123", "A1b", "veryveryveryverylongPassword123"
            };

            foreach (var badPass in badPasswords)
            {
                TestRegister("validation@test.com", badPass, false);
            }
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
        #endregion
    }
}