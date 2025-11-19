using System;
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class AuthenticationFacade
    {
        private readonly Dictionary<string, bool> loggedUsers;

        public AuthenticationFacade() {
            loggedUsers = new Dictionary<string, bool>();
        }

        public void Register(string email)
        {
            if (!loggedUsers.ContainsKey(email))
                loggedUsers.Add(email, false);
            else
                throw new Exception("email already exists");
        }

        public void Login(string email)
        {
            if (loggedUsers.ContainsKey(email))
            {
                loggedUsers[email] = true;
            }
            else
            {
                throw new Exception("user doesn't exist");
            }
        }

        public void Logout(string email) 
        {
            if (!loggedUsers.ContainsKey(email))
                throw new Exception("user doesn't exist");
            if (!loggedUsers[email]) 
                throw new Exception("user is not logged in");
            loggedUsers[email] = false;
        }

        public bool IsLoggedIn(string email)
        {
            if (loggedUsers.ContainsKey(email))
            {
                return loggedUsers[email];
            }
            else
            {
                throw new Exception("user doesn't exist");
            }
        }

        public bool IsEmailRegistered(string email)
        {
            return loggedUsers.ContainsKey(email);
        }

        public void Clear()
        {
            loggedUsers.Clear();
        }
    }


}