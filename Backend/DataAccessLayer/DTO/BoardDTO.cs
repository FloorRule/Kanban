namespace IntroSE.Kanban.Backend.DataAccessLayer.DTO
{
    internal class BoardDTO
    {
        public const string boardIdColumn = "Id";
        public const string boardNameColumn = "Name";
        public const string boardOwnerEmailColumn = "OwnerEmail";

        private readonly BoardController controller;
        private bool _isPersisted = false;

        public long Id { get; private set; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (_isPersisted)
                    controller.Update(Id, boardNameColumn, value);
            }
        }

        private string ownerEmail;
        public string OwnerEmail
        {
            get => ownerEmail;
            set
            {
                ownerEmail = value;
                if (_isPersisted)
                    controller.Update(Id, boardOwnerEmailColumn, value);
            }
        }

        public BoardDTO(long id, string name, string ownerEmail)
        {
            Id = id;
            this.name = name;
            this.ownerEmail = ownerEmail;
            controller = new BoardController();
        }
        public void MarkAsPersisted()
        {
            _isPersisted = true;
        }
    }
}
