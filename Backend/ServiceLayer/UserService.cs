
using System;
using System.Text.Json;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class UserService
    {

        private UserFacade userFacade;

        internal UserService(UserFacade userFacade) {
            this.userFacade = userFacade;
        }

        /// <summary>
        /// Registers a new user with the given email and password.
        /// 
        /// Preconditions: Email and password must not be empty. The email should not already be registered.
        /// Postconditions: A new user is created and logged in. Returns a success message or token.
        /// </summary>
        /// <param name="email">The email address of the user to register.</param>
        /// <param name="password">The password for the new account.</param>
        /// <returns>A success message or token if registration is successful.</returns>
        public string Register(string email, string password)
        {
            return HandleAuth(email, SystemAction.Register, password);

        }

        /// <summary>
        /// Logs in a user using their email and password.
        /// 
        /// Preconditions: Email and password must be provided. The user must exist and the password must be correct.
        /// Postconditions: The user is logged in and a session token or success message is returned.
        /// </summary>
        /// <param name="email">The email address of the user trying to log in.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A token or confirmation message if login is successful.</returns>
        public string Login(string email, string password)
        {
            return HandleAuth(email, SystemAction.Login, password);
        }


        /// <summary>
        /// Logs out the user associated with the given email.
        /// 
        /// Preconditions: Email must not be empty and the user must be logged in.
        /// Postconditions: The user is logged out and a confirmation message is returned.
        /// </summary>
        /// <param name="email">The email address of the user to log out.</param>
        /// <returns>A message confirming the user has been logged out.</returns>
        public string Logout(string email)
        {
            return HandleAuth(email, SystemAction.Logout);

        }

        private string HandleAuth(string email, SystemAction action, string password = "")
        {
            Response res;
            try
            {
                PerformAuthOperation(email, action, password);

                if (action == SystemAction.Login || action == SystemAction.Register)
                    res = new Response(email);
                else
                    res = new Response();
               
                return JsonSerializer.Serialize(res);
            }
            catch (Exception e)
            {
                res = new Response(e.Message, true);
                return JsonSerializer.Serialize(res);
            }
        }

        private void PerformAuthOperation(string email, SystemAction action, string password = "")
        {
            switch (action)
            {
                case SystemAction.Login:
                    this.userFacade.Login(email, password);
                    break;
                case SystemAction.Register:
                    this.userFacade.Register(email, password);
                    break;
                case SystemAction.Logout:
                    this.userFacade.Logout(email);
                    break;
                default:
                    throw new Exception("Non existed action");
            }
        }

        public string LoadData()
        {
            Response res;
            try
            {
                userFacade.LoadData();
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
                userFacade.DeleteData();
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
