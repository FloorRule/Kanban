using Frontend.Model;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows; // For MessageBox
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Frontend.ViewModel
{
    class MyBoardsViewModel : NotifiableObject
    {
        private readonly MainWindowViewModel mainVM;
        private BackendController Controller => mainVM.Controller;
        private readonly UserModel _user;

        public ObservableCollection<BoardModel> Boards { get; set; }
        public ObservableCollection<TaskModel> InProgressTasks { get; private set; }

        private string _chipName;
        public string ChipName
        {
            get => _chipName;
            set
            {
                _chipName = value;
                RaisePropertyChanged("ChipName");
            }
        }
        private string _iconColor;
        public string IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
                RaisePropertyChanged("IconColor");
            }
        }

        private string _emailToolTip;
        public string EmailToolTip
        {
            get => _emailToolTip;
            set
            {
                _emailToolTip = value;
                RaisePropertyChanged("EmailToolTip");
            }
        }

        private BoardModel _selectedBoard;
        public BoardModel SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                _selectedBoard = value;
                RaisePropertyChanged("SelectedBoard");
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    RaisePropertyChanged(nameof(SelectedTabIndex));
                    LoadBoards();
                }
            }
        }

        // --- Commands ---
        public ICommand LogoutCommand { get; private set; }
        public ICommand CreateBoardCommand { get; private set; }
        public ICommand ViewBoardCommand { get; private set; }
        public ICommand JoinBoardCommand { get; private set; }
        public ICommand DeleteBoardCommand { get; private set; }

        private int MAX_DISPLAY = 10;
        private int FIRST_INDEX = 0;
        private char EMAIL_SYMBOL = '@';

        public MyBoardsViewModel(UserModel user, MainWindowViewModel mainVM)
        {
            this._user = user;
            this.mainVM = mainVM;

            EmailToolTip = _user.Email;
            string username = _user.Email.Split(EMAIL_SYMBOL)[FIRST_INDEX];
            username = username.Length <= MAX_DISPLAY ? username : username.Substring(FIRST_INDEX, MAX_DISPLAY);

            ChipName = username;
            IconColor = ColorCode.colorPicker(username);

            // Initialize Commands
            LogoutCommand = new RelayCommand(Logout);
            ViewBoardCommand = new RelayCommand(ViewBoard, (p) => p is BoardModel);
            CreateBoardCommand = new RelayCommand(CreateBoard, (p) => p is TextBox);
            DeleteBoardCommand = new RelayCommand(DeleteBoard, (p) => p is string);
            JoinBoardCommand = new RelayCommand(JoinBoard, (p) => p is BoardModel);

            _selectedTabIndex = 0;
            
            LoadBoards();// Load the user's boards
        }

        private void LoadBoards()
        {
            try
            {
                Boards = new ObservableCollection<BoardModel>();
                InProgressTasks = new ObservableCollection<TaskModel>();

                if (SelectedTabIndex == 0) // "My Boards" tab
                    Boards = _user.GetUserBoards();
                else if (SelectedTabIndex == 1) // "Browse" tab
                    Boards = _user.GetAllBoards();
                else if (SelectedTabIndex == 2) // "In-Prog" tab 
                {
                    var userBoards = _user.GetUserBoards();
                    foreach (var board in userBoards)
                    {
                        board.LoadColumns();
                        if (board.InProgressColumn != null)
                        {
                            foreach (var task in board.InProgressColumn.Tasks)
                            {
                                if(task.Assignee == _user.Email)
                                    InProgressTasks.Add(task);
                            }
                        }
                    }
                }
                   

                RaisePropertyChanged(nameof(Boards));
                RaisePropertyChanged(nameof(InProgressTasks));
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not load boards: {e.Message}");

                Boards = new ObservableCollection<BoardModel>();
                InProgressTasks = new ObservableCollection<TaskModel>();
                RaisePropertyChanged(nameof(Boards));
                RaisePropertyChanged(nameof(InProgressTasks));
            }
        }

        private void ViewBoard(object parameter)
        {
            if (parameter is BoardModel board)
            {
                try
                {
                    board.LoadColumns();

                    mainVM.NavigateToBoard(_user, board);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Could not open board: {e.Message}");
                }
            }
        }

        private void Logout(object parameter)
        {
            try
            {
                Controller.Logout(_user.Email);

                mainVM.NavigateToLogin();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Logout failed: {e.Message}");
            }
        }

        private void CreateBoard(object parameter)
        {
            try
            {
                if (parameter is TextBox boardName && !string.IsNullOrEmpty(boardName.Text))
                {
                    Controller.CreateBoard(_user, boardName.Text);
                    LoadBoards();

                    PopupBox.ClosePopupCommand.Execute(null, null);
                    boardName.Text = "";
                    MessageBox.Show("Created Board Successfuly");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Create Board failed: {e.Message}");
            }
        }

        private void DeleteBoard(object parameter)
        {
            try
            {
                if (parameter is string boardName)
                {
                    Controller.DeleteBoard(_user, boardName);
                    LoadBoards();

                    MessageBox.Show("Deleted Board Successfuly");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Delete Board failed: {e.Message}");
            }
        }

        private void JoinBoard(object parameter)
        {
            try
            {
                if (parameter is BoardModel board)
                {
                    Controller.JoinBoard(_user, board.Id);
                    LoadBoards();

                    MessageBox.Show("Joined Board Successfuly");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Join Board failed: {e.Message}");
            }
        }

    }
}