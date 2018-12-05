using Utility;

namespace Models.EquipmentMaintenance.MonthlyReport
{
    public class CheckResultModel
    {
        public int Day
        {
            get
            {
                return DateTimeHelper.DateString2DateTime(CheckDate).Value.Day;
            }
        }

        public string CheckDate { get; set; }

        public string Result { get; set; }
    }
}
