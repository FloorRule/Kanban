using Frontend.Model;
using System;

namespace Frontend.ViewModel
{
    public class MainWindowViewModel : NotifiableObject
    {
        public BackendController Controller { get; private set; }

        private NotifiableObject _currentViewModel;
        public NotifiableObject CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public MainWindowViewModel(BackendController controller)
        {
            this.Controller = controller;
            // Login screen
            NavigateToLogin();
        }

        public void NavigateToLogin()
        {
            CurrentViewModel = new LoginViewModel(this);
        }

        public void NavigateToRegister()
        {
            CurrentViewModel = new RegisterViewModel(this);
        }

        public void NavigateToMyBoards(UserModel user)
        {
            CurrentViewModel = new MyBoardsViewModel(user, this);
        }

        public void NavigateToBoard(UserModel user, BoardModel board)
        {
            CurrentViewModel = new BoardViewModel(user, board, this);
        }
    }
}