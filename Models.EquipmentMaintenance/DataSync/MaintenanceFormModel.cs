using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.DataSync
{
    public class MaintenanceFormModel
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Description { get; set; }

        public string Remark { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public DateTime BeginDate { get; set; }

        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateString(BeginDate);
            }
        }

        public DateTime EndDate { get; set; }

        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateString(BeginDate);
            }
        }

        public List<StandardModel> StandardList { get; set; }

        public List<MFormMaterialModel> MaterialList { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList
        {
            get
            {
                return StandardList.SelectMany(x => x.AbnormalReasonList).ToList();
            }
        }

        public List<UserModel> UserList { get; set; }

        public MaintenanceFormModel()
        {
            StandardList = new List<StandardModel>();
            MaterialList = new List<MFormMaterialModel>();
            UserList = new List<UserModel>();
        }
    }
}
