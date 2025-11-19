using Frontend.Model;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontend.ViewModel
{
    class LoginViewModel : NotifiableObject
    {
        private MainWindowViewModel mainVM;
        private BackendController Controller => mainVM.Controller;

        private string _username;
        public string Username
        {
            get => _username;
            set 
            { 
                _username = value; 
                RaisePropertyChanged("Username"); 
            }
        }

        private string _password;

        private string _message;
        public string Message
        {
            get => _message;
            set 
            { 
                _message = value; 
                RaisePropertyChanged("Message"); 
            }
        }

        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterNavCommand { get; private set; }

        public LoginViewModel(MainWindowViewModel mainVM)
        {
            this.mainVM = mainVM;
            LoginCommand = new RelayCommand(Login);
            RegisterNavCommand = new RelayCommand((p) => mainVM.NavigateToRegister());
        }

        private void Login(object parameter)
        {
            Message = "";
            if (parameter is PasswordBox passwordBox)
                _password = passwordBox.Password;
            try
            {
                UserModel user = Controller.Login(Username, _password);
                mainVM.NavigateToMyBoards(user);
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
        }
    }
}