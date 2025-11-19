using Frontend.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Frontend.ViewModel
{
    class BoardViewModel : NotifiableObject
    {
        private readonly MainWindowViewModel mainVM;
        private readonly UserModel _user;

        // --- Binding ---
        public BoardModel Board { get; private set; }
        public List<string> AssignableMembers { get; private set; }
        public string ChipName { get; private set; }
        public string IconColor { get; private set; }
        public bool IsOwner { get; private set; }
        public string LoggedInUserEmail => _user.Email;
        public bool IsBoardMember => IsOwner || Board.Members.Contains(_user.Email);
        // --- Add Task ---
        private string _newTaskTitle;
        public string NewTaskTitle 
        { 
            get => _newTaskTitle; 
            set { _newTaskTitle = value; RaisePropertyChanged(nameof(NewTaskTitle)); } 
        }

        private string _newTaskDescription;
        public string NewTaskDescription 
        { 
            get => _newTaskDescription; 
            set { _newTaskDescription = value; RaisePropertyChanged(nameof(NewTaskDescription)); } 
        }

        private DateTime? _newTaskDueDate = DateTime.Now.AddDays(7);
        public DateTime? NewTaskDueDate 
        { 
            get => _newTaskDueDate; 
            set { _newTaskDueDate = value; RaisePropertyChanged(nameof(NewTaskDueDate)); } 
        }

        // --- Commands ---
        public ICommand LogoutCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand AddTaskCommand { get; private set; }
        public ICommand LeaveBoardCommand { get; private set; }
        public ICommand TransferOwnershipCommand { get; private set; }
        public ICommand MoveTaskCommand { get; private set; }
        public ICommand DeleteTaskCommand { get; private set; }

        private int MAX_DISPLAY = 10;
        private int FIRST_INDEX = 0;
        private char EMAIL_SYMBOL = '@';
        public BoardViewModel(UserModel user, BoardModel board, MainWindowViewModel mainVM)
        {
            this.mainVM = mainVM;
            this._user = user;
            this.Board = board;

            this.AssignableMembers = new List<string>(board.Members);

            if (!this.AssignableMembers.Contains(board.OwnerEmail))
                this.AssignableMembers.Add(board.OwnerEmail);

            this.AssignableMembers.Insert(FIRST_INDEX, null);

            // Determine if the current user is the owner for UI visibility
            IsOwner = _user.Email.Equals(board.OwnerEmail, StringComparison.OrdinalIgnoreCase);

            // User chip display
            string username = user.Email.Split(EMAIL_SYMBOL)[FIRST_INDEX];
            ChipName = username.Length <= MAX_DISPLAY ? username : username.Substring(FIRST_INDEX, MAX_DISPLAY);
            IconColor = ColorCode.colorPicker(username);

            // Initialize commands
            LogoutCommand = new RelayCommand((p) => mainVM.NavigateToLogin());
            BackCommand = new RelayCommand((p) => mainVM.NavigateToMyBoards(_user));
            AddTaskCommand = new RelayCommand(AddTask, (p) => !string.IsNullOrWhiteSpace(NewTaskTitle));
            LeaveBoardCommand = new RelayCommand(LeaveBoard, (p) => !IsOwner); // Can only leave if not the owner
            TransferOwnershipCommand = new RelayCommand(TransferOwnership, (p) => IsOwner && p is string); // Can only transfer if owner
            MoveTaskCommand = new RelayCommand(MoveTask, (p) => p is TaskModel);
            DeleteTaskCommand = new RelayCommand(DeleteTask, (p) => p is TaskModel);
        }

        private void AddTask(object parameter)
        {
            try
            {
                Board.Controller.AddTask(_user.Email, Board.Name, NewTaskTitle, NewTaskDescription, NewTaskDueDate ?? DateTime.Now);
                Board.LoadColumns(); // Refresh board

                PopupBox.ClosePopupCommand.Execute(null, null);
                NewTaskTitle = ""; 
                NewTaskDescription = ""; 
                NewTaskDueDate = DateTime.Now.AddDays(7);
                MessageBox.Show("Task created successfully!");
            }
            catch (Exception e) { MessageBox.Show($"Failed to create task: {e.Message}"); }
        }

        private void LeaveBoard(object parameter)
        {
            try
            {
                Board.Controller.LeaveBoard(_user, Board.Id);
                MessageBox.Show("Leaving board successfully!");
                mainVM.NavigateToMyBoards(_user);
            }
            catch (Exception e) { MessageBox.Show($"Failed to leave board: {e.Message}"); }
        }

        private void TransferOwnership(object parameter)
        {
            if (parameter is string newOwnerEmail)
            {
                try
                {
                    Board.Controller.TransferOwnership(_user, newOwnerEmail, Board.Name);

                    int boardId = this.Board.Id;
                    BoardModel refreshedBoard = mainVM.Controller.GetBoard(_user, boardId);

                    refreshedBoard.LoadColumns();
                    MessageBox.Show("Ownership transferred successfully");

                    mainVM.NavigateToBoard(_user, refreshedBoard);
                }
                catch (Exception e) { MessageBox.Show($"Failed to transfer ownership: {e.Message}"); }
            }
        }

        private void MoveTask(object parameter)
        {
            if (parameter is TaskModel task)
            {
                try
                {
                    task.Controller.MoveTask(_user.Email, Board.Name, (int)task.Id);
                    Board.LoadColumns(); // Refresh all columns
                    MessageBox.Show("Task moved successfully");
                }
                catch (Exception e) { MessageBox.Show($"Could not move task: {e.Message}"); }
            }
        }

        private void DeleteTask(object parameter)
        {
            if (parameter is TaskModel task)
            {
                try
                {
                    task.Controller.DeleteTask(_user.Email, Board.Name, (int)task.Id);
                    Board.LoadColumns(); // Refresh all columns
                    MessageBox.Show("Task deleted successfully");
                }
                catch (Exception e) { MessageBox.Show($"Could not delete task: {e.Message}"); }
            }
        }
    }
}