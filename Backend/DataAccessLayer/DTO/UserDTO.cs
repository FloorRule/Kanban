
namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class UserDTO
    {
        public const string userEmailColumnName = "Email";
        public const string userPasswordColumnName = "Password";
        private readonly UserController controller;
        private bool _isPersisted = false;

        public string Email { get; private set; }


        private string password;
        public string Password
        {
            get => password;
            set
            {
                if (_isPersisted)
                    controller.Update(Email, userPasswordColumnName, value);
                password = value;
            }
        }

        public UserDTO(string email, string password)
        {
            Email = email;
            this.password = password;
            controller = new UserController();
        }
        public void MarkAsPersisted()
        {
            _isPersisted = true;
        }
    }
}
