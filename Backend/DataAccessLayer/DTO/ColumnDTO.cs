namespace IntroSE.Kanban.Backend.DataAccessLayer.DTO
{
    internal class ColumnDTO
    {
        public const string ColumnsBoardIdColumnName = "BoardID";
        public const string ColumnsOrdinalColumnName = "Ordinal";
        public const string ColumnsLimitColumnName = "TasksLimit";

        private readonly ColumnController controller;
        private bool _isPersisted = false;

        public int BoardID { get; private set; }
        public int ColumnOrdinal { get; private set; }

        private int _tasksLimit;
        public int TasksLimit
        {
            get => _tasksLimit;
            set
            {
                _tasksLimit = value;
                if (_isPersisted)
                    controller.Update(BoardID, ColumnOrdinal, ColumnsLimitColumnName, value);
            }
        }

        public ColumnDTO(int boardId, int ordinal, int tasksLimit)
        {
            controller = new ColumnController();
            BoardID = boardId;
            ColumnOrdinal = ordinal;
            _tasksLimit = tasksLimit;
        }

        public void MarkAsPersisted()
        {
            _isPersisted = true;
        }
    }
}
