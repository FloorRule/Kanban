using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class ColumnSL
    {
        private LinkedList<TaskSL> tasks;
        public IReadOnlyList<TaskSL> Tasks { get => tasks.ToList().AsReadOnly(); }
        internal ColumnSL(ColumnBL colBL)
        {
            this.tasks = new LinkedList<TaskSL>();
            if (colBL != null && colBL.Tasks != null)
            {
                foreach (var task in colBL.Tasks)
                    this.tasks.AddLast(new TaskSL(task));
            }
        }

    }
}
