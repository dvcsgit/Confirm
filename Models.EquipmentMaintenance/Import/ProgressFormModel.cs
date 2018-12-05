using System.Linq;
using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace Models.EquipmentMaintenance.Import
{
    public class ProgressFormModel
    {
        public IWorkbook Workbook { get; set; }

        public Dictionary<int, int> SheetDataCount { get; set; }

        public int TotalCount
        {
            get
            {
                return SheetDataCount.Values.Sum();
            }
        }

        public RowData RowData { get; set; }

        public ProgressFormModel()
        {
            SheetDataCount = new Dictionary<int, int>();
            RowData = new RowData();
        }
    }
}
