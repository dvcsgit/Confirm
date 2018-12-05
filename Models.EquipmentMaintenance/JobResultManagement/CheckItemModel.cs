using System.Collections.Generic;
using System.Linq;

namespace Models.EquipmentMaintenance.JobResultManagement
{
    public class CheckItemModel
    {
        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string CheckItemUniqueID { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public bool HaveAbnormal
        {
            get
            {
                return CheckResultList.Any(x => x.IsAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return CheckResultList.Any(x => x.IsAlert);
            }
        }

        public bool IsChecked
        {
            get
            {
                return CheckResultList.Count > 0;
            }
        }

        public CheckItemModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
