using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class ResultModel
    {
        public string UniqueID { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string DateTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(Date, Time));
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public string Remark { get; set; }

        public StandardResultModel Result { get; set; }

        public ResultModel()
        {
            Result = new StandardResultModel();
        }
    }
}
