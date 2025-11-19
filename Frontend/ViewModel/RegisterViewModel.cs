// In file: ViewModel/RegisterViewModel.cs
using Frontend.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontend.ViewModel
{
    class RegisterViewModel : NotifiableObject
    {
        private readonly MainWindowViewModel mainVM;
        private BackendController Controller => mainVM.Controller;

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; RaisePropertyChanged("Email"); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; RaisePropertyChanged("Message"); }
        }

        // --- Commands ---
        public ICommand RegisterCommand { get; private set; }
        public ICommand BackToLoginCommand { get; private set; }

        public RegisterViewModel(MainWindowViewModel mainVM)
        {
            this.mainVM = mainVM;

            RegisterCommand = new RelayCommand(Register);
            BackToLoginCommand = new RelayCommand((p) => mainVM.NavigateToLogin());
        }

        private void Register(object parameter)
        {
            Message = "";
            try
            {
                // The parameter is a Tuple of the two PasswordBox controls from the View
                if (parameter is Tuple<object, object> passwordBoxes)
                {
                    var passBox1 = passwordBoxes.Item1 as PasswordBox;
                    var passBox2 = passwordBoxes.Item2 as PasswordBox;

                    if (passBox1 == null || passBox2 == null)
                    {
                        Message = "Passwords boxes are empty.";
                        return;
                    }

                    string password = passBox1.Password;
                    string confirmPassword = passBox2.Password;

                    // --- Validation ---
                    if (password != confirmPassword)
                    {
                        Message = "Passwords do not match.";
                        return;
                    }

                    // --- Backend Call ---
                    UserModel user = Controller.Register(Email, password);

                    MessageBox.Show("Registration Successful!");
                    mainVM.NavigateToMyBoards(user);
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
        }
    }
}