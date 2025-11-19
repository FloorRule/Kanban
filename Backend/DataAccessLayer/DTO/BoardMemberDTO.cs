namespace IntroSE.Kanban.Backend.DataAccessLayer.DTO
{
    internal class BoardMemberDTO
    {
        public const string BoardIdColumn = "BoardID";
        public const string EmailColumn = "Email";

        public int BoardID { get; set; }
        public string Email { get; set; }

        public BoardMemberDTO(int boardId, string email)
        {
            BoardID = boardId;
            Email = email;
        }
    }
}