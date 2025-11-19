using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class Convertor
    {
        /// <summary>
        /// Converts a dictionary of BoardBL objects to a dictionary of BoardSL objects,
        /// preserving the string keys (e.g., board names).
        /// 
        /// Preconditions: The input dictionary must not be null and must contain valid BoardBL objects.
        /// Postconditions: Returns a dictionary of the same size with each key mapped to a new
        ///                 BoardSL object constructed from the corresponding BoardBL.
        /// </summary>
        /// <param name="dictBL">A dictionary mapping string keys to BoardBL objects.</param>
        /// <returns>A dictionary mapping string keys to BoardSL objects.</returns>
        internal static Dictionary<string, BoardSL> boardConverterSL(Dictionary<string, BoardBL> dictBL)
        {
            Dictionary<string, BoardSL> dictSL = new Dictionary<string, BoardSL>();
            foreach (var boardPair in dictBL)
                dictSL[boardPair.Key] = new BoardSL(boardPair.Value);
            return dictSL;
        }

        /// <summary>
        /// Converts a dictionary of ColumnBL objects to a dictionary of ColumnSL objects,
        /// preserving the ColumnType keys.
        ///
        /// Preconditions: The input dictionary must not be null and must contain valid ColumnBL objects.
        /// Postconditions: Returns a dictionary of the same size with each key mapped to a new
        ///                 ColumnSL object constructed from the corresponding ColumnBL.
        /// </summary>
        /// <param name="dictBL">A dictionary mapping ColumnType to ColumnBL objects.</param>
        /// <returns>A dictionary mapping ColumnType to ColumnSL objects.</returns>
        internal static Dictionary<ColumnType, ColumnSL> columnConverterSL(Dictionary<ColumnType, ColumnBL> dictBL)
        {
            Dictionary<ColumnType, ColumnSL> dictSL = new Dictionary<ColumnType, ColumnSL>();
            foreach (var colPair in dictBL)
                dictSL[colPair.Key] = new ColumnSL(colPair.Value);
            return dictSL;
        }

        /// <summary>
        /// Converts a linked list of TaskBL objects to a linked list of TaskSL objects.
        /// 
        /// Preconditions: The input list must not be null and must contain valid TaskBL objects.
        /// Postconditions: Returns a linked list of the same length containing TaskSL objects 
        ///                 corresponding to each TaskBL object in the input list.
        /// </summary>
        /// <param name="listBL">A linked list of TaskBL objects to be converted.</param>
        /// <returns>A linked list of TaskSL objects.</returns>
        internal static LinkedList<TaskSL> taskConverterSL(LinkedList<TaskBL> listBL)
        {
            LinkedList<TaskSL> listSL = new LinkedList<TaskSL>();
            foreach (var task in listBL)
            {
                listSL.AddLast(new TaskSL(task));
            }
            return listSL;
        }

    }
}
