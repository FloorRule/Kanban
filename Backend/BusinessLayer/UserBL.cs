
using System;
using System.Linq;
using System.Text.RegularExpressions;
using IntroSE.Kanban.Backend.DataAccessLayer;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserBL
    {
        private readonly string password;
        private string email;

        public UserDTO userDTO;

        public UserBL(string email, string password)
        {
            this.Email = email;
            if (!IsValidPassword(password))
                throw new Exception("Invalid password");
            this.password = password;
        }

        public UserDTO UserDTO
        {
            set { userDTO = value; }
        }

        public string Email
        {
            get => email;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("Email cannot be empty");

                string lower = value.ToLowerInvariant();

                // This regex is based on the general standard of RFC 5322 (see Wikipedia link)
                string emailPattern =
                    @"^(?("")("".+?""@)|(([0-9a-zA-Z](([\.\-]?[0-9a-zA-Z])*)@)))" +
                    @"(([0-9a-zA-Z][\w\-]*\.)+[a-zA-Z]{2,})$";

                if (!Regex.IsMatch(lower, emailPattern))
                    throw new Exception("Invalid email");

                email = lower;
            }
        }

        private bool IsValidPassword(string password)
        {
            if(string.IsNullOrEmpty(password))
                return false;
            int minPassLength = 6;
            int maxPassLength = 20;
            bool isInvalidLength = password.Length > maxPassLength || password.Length < minPassLength;
            bool isInvalidChars = !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit);

            return !(isInvalidLength || isInvalidChars);
        }


        public bool Login(string password)
        {
            return this.password.Equals(password);
        }

    }
}
