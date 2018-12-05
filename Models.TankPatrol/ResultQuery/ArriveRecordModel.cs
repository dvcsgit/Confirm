using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.ResultQuery
{
    public class ArriveRecordModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string StationID { get; set; }

        public string StationDescription { get; set; }

        public string Station
        {
            get
            {
                return string.Format("{0}/{1}", StationID, StationDescription);
            }
        }

        public string IslandID { get; set; }

        public string IslandDescription { get; set; }

        public string Island
        {
            get
            {
                return string.Format("{0}/{1}", IslandID, IslandDescription);
            }
        }

        public string PortID { get; set; }

        public string PortDescription { get; set; }

        public string Port
        {
            get
            {
                return string.Format("{0}/{1}", PortID, PortDescription);
            }
        }

        public string CheckType { get; set; }

        public string CheckTypeDisplay
        {
            get
            {
                if (CheckType == "U")
                {
                    return "卸料";
                }
                else if (CheckType == "L")
                {
                    return "裝料";
                }
                else
                {
                    return "-";
                }
            }
        }

        public string TankNo { get; set; }

        public string Driver { get; set; }

        public string LastTimeMaterial { get; set; }

        public string ThisTimeMaterial { get; set; }

        public string Owner { get; set; }

        public string UnRFIDReasonDescription { get; set; }

        public string UnRFIDReasonRemark { get; set; }

        public string UnRFIDReason
        {
            get
            {
                if (!string.IsNullOrEmpty(UnRFIDReasonDescription))
                {
                    return UnRFIDReasonDescription;
                }
                else
                {
                    return UnRFIDReasonRemark;
                }
            }
        }

        public string SignExtension { get; set; }

        public string Sign
        {
            get
            {
                if (!string.IsNullOrEmpty(SignExtension))
                {
                    return string.Format("{0}.{1}", UniqueID, SignExtension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

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

        public List<CheckResultModel> LBCheckResultList { get; set; }
        public List<CheckResultModel> LPCheckResultList { get; set; }
        public List<CheckResultModel> LACheckResultList { get; set; }
        public List<CheckResultModel> LDCheckResultList { get; set; }

        public List<CheckResultModel> UBCheckResultList { get; set; }
        public List<CheckResultModel> UPCheckResultList { get; set; }
        public List<CheckResultModel> UACheckResultList { get; set; }
        public List<CheckResultModel> UDCheckResultList { get; set; }
    }
}
