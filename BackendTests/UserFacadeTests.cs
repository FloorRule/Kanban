using NUnit.Framework;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer;
using System;
using System.IO;

namespace BackendTests
{
    [TestFixture]
    public class UserFacadeTests
    {
        private UserFacade userFacade;
        private AuthenticationFacade auth;

        [SetUp]
        public void Setup()
        {
            TestDbBoot.EnsureSchemaExists();

            auth = new AuthenticationFacade();
            userFacade = new UserFacade(auth);
        }

        [TearDown]
        public void TearDown()
        {
            // Just cleaning everything at the end so tests don’t mess with each other
            userFacade.DeleteData();
        }

        // ============================
        // SECTION 1: Register & Login
        // ============================

        [Test]
        public void Register_ValidUser_DoesNotThrowAndUserIsLoggedIn()
        {
            // Registering should work fine and log the user in
            Assert.DoesNotThrow(() => userFacade.Register("valid@example.com", "GoodPass1"));
            Assert.IsTrue(auth.IsLoggedIn("valid@example.com"));
        }

        [Test]
        public void Register_DuplicateEmail_ThrowsException()
        {
            // Trying to register the same email twice should crash
            userFacade.Register("duplicate@example.com", "GoodPass1");
            var ex = Assert.Throws<Exception>(() => userFacade.Register("duplicate@example.com", "AnotherPass1"));
            Assert.That(ex.Message, Is.EqualTo("Email already exists"));
        }

        [TestCase("bad")]
        [TestCase("short1")]
        [TestCase("NOCAPS123")]
        [TestCase("nocaps123")]
        [TestCase("NoDigitsHere")]
        [TestCase(null)]
        public void Register_InvalidPassword_ThrowsException(string password)
        {
            // All these passwords are not strong enough → should throw
            var ex = Assert.Throws<Exception>(() => userFacade.Register("invalidpass@example.com", password));
            Assert.That(ex.Message, Is.EqualTo("illegal password"));
        }

        [Test]
        public void Login_ValidCredentialsAfterLogout_Succeeds()
        {
            // Should be able to log in again after logging out
            userFacade.Register("login@example.com", "GoodPass1");
            userFacade.Logout("login@example.com");
            Assert.DoesNotThrow(() => userFacade.Login("login@example.com", "GoodPass1"));
            Assert.IsTrue(auth.IsLoggedIn("login@example.com"));
        }

        [Test]
        public void Login_WrongPassword_ThrowsException()
        {
            // Logging in with a wrong password should fail
            userFacade.Register("wrongpass@example.com", "GoodPass1");
            userFacade.Logout("wrongpass@example.com");
            var ex = Assert.Throws<Exception>(() => userFacade.Login("wrongpass@example.com", "WrongPass1"));
            Assert.That(ex.Message, Is.EqualTo("Wrong password"));
        }

        [Test]
        public void Login_AlreadyLoggedIn_ThrowsException()
        {
            // If the user is already logged in, they shouldn’t be able to log in again
            userFacade.Register("alreadyloggedin@example.com", "GoodPass1");
            var ex = Assert.Throws<Exception>(() => userFacade.Login("alreadyloggedin@example.com", "GoodPass1"));
            Assert.That(ex.Message, Is.EqualTo("User is already logged in"));
        }

        [Test]
        public void Logout_ValidUser_Succeeds()
        {
            // Logout should work for logged-in users
            userFacade.Register("logout@example.com", "GoodPass1");
            Assert.DoesNotThrow(() => userFacade.Logout("logout@example.com"));
            Assert.IsFalse(auth.IsLoggedIn("logout@example.com"));
        }

        [Test]
        public void Logout_NotLoggedIn_ThrowsException()
        {
            // Trying to log out someone who already logged out should throw
            userFacade.Register("notloggedin@example.com", "GoodPass1");
            userFacade.Logout("notloggedin@example.com");
            var ex = Assert.Throws<Exception>(() => userFacade.Logout("notloggedin@example.com"));
            Assert.That(ex.Message, Is.EqualTo("user is not logged in"));
        }

        // =======================
        // SECTION 2: Persistence
        // =======================

        [Test]
        public void LoadData_Restores_Multiple_Users_AsLoggedOut()
        {
            // Registering users (they’re all logged in now)
            userFacade.Register("user1@example.com", "GoodPass1");
            userFacade.Register("user2@example.com", "GoodPass2");
            userFacade.Register("user3@example.com", "GoodPass3");

            // Simulate app restart by making a new facade instance and loading from DB
            var newAuth = new AuthenticationFacade();
            var newFacade = new UserFacade(newAuth);
            Assert.DoesNotThrow(() => newFacade.LoadData());

            // Users should exist but not be logged in after loading from DB
            Assert.IsTrue(newAuth.IsEmailRegistered("user1@example.com"));
            Assert.IsFalse(newAuth.IsLoggedIn("user1@example.com"));
            Assert.IsFalse(newAuth.IsLoggedIn("user2@example.com"));
            Assert.IsFalse(newAuth.IsLoggedIn("user3@example.com"));

            // Try logging one of them back in to be sure it works
            Assert.DoesNotThrow(() => newFacade.Login("user1@example.com", "GoodPass1"));
            Assert.IsTrue(newAuth.IsLoggedIn("user1@example.com"));
        }

        [Test]
        public void Data_Survives_Across_Instances_Login()
        {
            // Register a user, then simulate a fresh app session
            userFacade.Register("persistent@user.com", "StrongPassword123");

            var freshAuth = new AuthenticationFacade();
            var freshUserFacade = new UserFacade(freshAuth);

            // Must load the old data to find the user
            Assert.DoesNotThrow(() => freshUserFacade.LoadData());

            // Try logging in using the new instance
            Assert.DoesNotThrow(() => freshUserFacade.Login("persistent@user.com", "StrongPassword123"));
            Assert.IsTrue(freshAuth.IsLoggedIn("persistent@user.com"));
        }

        [Test]
        public void DeleteData_Clears_All_Users_And_Auth_State()
        {
            // Register a user and then delete everything
            userFacade.Register("tobedeleted@user.com", "GoodPass1");
            userFacade.DeleteData();

            // The user should no longer exist or be able to log in
            var ex = Assert.Throws<Exception>(() => userFacade.Login("tobedeleted@user.com", "GoodPass1"));
            Assert.AreEqual("User doesn't exist", ex.Message);
            Assert.IsFalse(auth.IsEmailRegistered("tobedeleted@user.com"));

            // Also test by reloading into a new instance
            var newAuth = new AuthenticationFacade();
            var newFacade = new UserFacade(newAuth);
            newFacade.LoadData();

            Assert.IsFalse(newAuth.IsEmailRegistered("tobedeleted@user.com"));
        }
    }
}
