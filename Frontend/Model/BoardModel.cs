using Frontend.Model;
using Frontend.ViewModel;

public class BoardModel : NotifiableModelObject
{
    private readonly UserModel _user;
    private readonly UserModel _ownerModel;
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Owner { get; private set; }
    public string OwnerEmail { get; private set; }
    public string CardColor { get; private set; }
    public bool CanJoin { get; private set; }
    public List<string> Members { get; private set; }
    public ColumnModel BacklogColumn { get; set; }
    public ColumnModel InProgressColumn { get; set; }
    public ColumnModel DoneColumn { get; set; }

    public BoardModel(BackendController controller, UserModel loggedInUser, UserModel ownerModel, int id, string name, List<string> members) : base(controller)
    {
        _user = loggedInUser;
        _ownerModel = ownerModel;
        Id = id;
        Name = name;
        Owner = "Owned By: " + _ownerModel.Email;
        OwnerEmail = _ownerModel.Email;
        CardColor = ColorCode.colorPicker(Name);
        CanJoin = false;
        Members = members;

        BacklogColumn = null;
        InProgressColumn = null;
        DoneColumn = null;
    }

    public void CheckJoinability(string email)
    {
        CanJoin = Owner != email && !Members.Contains(email);
    }

    public void LoadColumns()
    {
        BacklogColumn = new ColumnModel(Controller, _ownerModel, Name, 0, "Back-Log");
        InProgressColumn = new ColumnModel(Controller, _ownerModel, Name, 1, "In-Progress");
        DoneColumn = new ColumnModel(Controller, _ownerModel, Name, 2, "Done");

        RaisePropertyChanged(nameof(BacklogColumn));
        RaisePropertyChanged(nameof(InProgressColumn));
        RaisePropertyChanged(nameof(DoneColumn));
    }
}