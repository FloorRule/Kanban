using System;
using IntroSE.Kanban.Backend.ServiceLayer;
using IntroSE.Kanban.Backend.BusinessLayer;
using System.Windows;

namespace Frontend.Model
{
    public class TaskModel : NotifiableModelObject
    {
        // backend calls
        private readonly string _boardName;
        private readonly int _columnOrdinal;
        private readonly UserModel _currentUser;

        public string BoardName { get => _boardName; }
        public long Id { get; private set; }
        public DateTime CreationTime { get; private set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                Controller.UpdateTask(_currentUser.Email, _boardName, _columnOrdinal, (int)Id, SystemAction.UpdateTitle, value);
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                Controller.UpdateTask(_currentUser.Email, _boardName, _columnOrdinal, (int)Id, SystemAction.UpdateDescription, value);
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        private string _color;
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        private DateTime _dueDate;
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                Controller.UpdateTask(_currentUser.Email, _boardName, _columnOrdinal, (int)Id, SystemAction.UpdateDueDate, value);
                _dueDate = value;
                RaisePropertyChanged("DueDate");
            }
        }
        private bool _isInitializing = true;

        private string _assignee;
        public string Assignee
        {
            get => _assignee;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || _assignee == value)
                    return;

                _assignee = value;
                RaisePropertyChanged(nameof(Assignee));

                if (_isInitializing || string.IsNullOrWhiteSpace(value))
                    return;

                try
                {
                    Controller.AssignTask(_currentUser.Email, _boardName, _columnOrdinal, (int)Id, value);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        () => MessageBox.Show($"Could not assign task: {ex.Message}")
                    );
                }
            }
        }

        private string[] colorWheel = { "#FFF3CCCC", "#FFF3F3CC", "#FFCCF3CD" };

        public TaskModel(BackendController controller, TaskSL taskSL, string boardName, int columnOrdinal, UserModel currentUser) : base(controller)
        {
            _boardName = boardName;
            _columnOrdinal = columnOrdinal;
            _currentUser = currentUser;

            Color = colorWheel[_columnOrdinal];

            Id = taskSL.Id;
            CreationTime = taskSL.CreationTime;
            _title = taskSL.Title;
            _description = taskSL.Description;
            _dueDate = taskSL.DueDate;
            _assignee = taskSL.Assignee;
            _isInitializing = false;
        }
    }
}
