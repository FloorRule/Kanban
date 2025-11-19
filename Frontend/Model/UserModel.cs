using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.Model
{
    public class UserModel : NotifiableModelObject
    {
        private string _email;
        public string Email
        {
            get => _email;
            set 
            { 
                _email = value; 
                RaisePropertyChanged("Email"); 
            }
        }

        public UserModel(BackendController controller, string email) : base(controller)
        {
            this.Email = email;
        }

        public ObservableCollection<BoardModel> GetUserBoards()
        {
            return new ObservableCollection<BoardModel>(Controller.GetUserBoards(this));
        }
        public ObservableCollection<BoardModel> GetAllBoards()
        {
            return new ObservableCollection<BoardModel>(Controller.GetAllBoards(this));
        }
    }
}
