using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    internal class BoardSL
    {
        private readonly string name;
        private readonly Dictionary<ColumnType, ColumnSL> boardColumns;

        private int boardId;
        private Dictionary<string, bool> boardMembers;

        internal BoardSL(BoardBL boardBL)
        {
            this.name = boardBL.Name;
            this.boardId = boardBL.BoardId;
            this.boardColumns = new Dictionary<ColumnType, ColumnSL>{
                { ColumnType.BackLog, new ColumnSL(boardBL.GetColumn(ColumnType.BackLog)) },
                { ColumnType.InProgress, new ColumnSL(boardBL.GetColumn(ColumnType.InProgress)) },
                { ColumnType.Done, new ColumnSL(boardBL.GetColumn(ColumnType.Done)) }
            };
            this.boardMembers = boardBL.BoardMembers;
        }
    }
}
