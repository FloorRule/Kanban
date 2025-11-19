using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Frontend.Model
{
    public class ColumnModel : NotifiableModelObject
    {
        public string Name { get; private set; }
        public int Ordinal { get; private set; }
        public ObservableCollection<TaskModel> Tasks { get; set; }

        public ColumnModel(BackendController controller, UserModel user, string boardName, int ordinal, string name) : base(controller)
        {
            Name = name;
            Ordinal = ordinal;

            Tasks = new ObservableCollection<TaskModel>(Controller.GetColumnTasks(user, boardName, ordinal));
        }
    }
}
