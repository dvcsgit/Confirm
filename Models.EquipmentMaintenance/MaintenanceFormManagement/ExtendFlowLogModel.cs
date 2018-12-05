using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class ExtendFlowLogModel
    {
        public int Seq { get; set; }

        public string UserID { get; set; }

        public DateTime NotifyTime { get; set; }

        public string NotifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(NotifyTime);
            }
        }

        public DateTime? VerifyTime { get; set; }

        public string VerifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(VerifyTime);
            }
        }

        public bool? IsReject { get; set; }

        public string Result
        {
            get
            {
                if (!IsReject.HasValue)
                {
                    return string.Empty;
                }
                else
                {
                    if (IsReject.Value)
                    {
                        return Resources.Resource.Reject;
                    }
                    else
                    {
                        return Resources.Resource.Approve;
                    }
                }
            }
        }

        public string Remark { get; set; }
    }
}
