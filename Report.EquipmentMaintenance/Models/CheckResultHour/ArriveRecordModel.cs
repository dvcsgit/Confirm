using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.CheckResultHour
{
    public class ArriveRecordModel
    {
        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public ArriveRecordModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
