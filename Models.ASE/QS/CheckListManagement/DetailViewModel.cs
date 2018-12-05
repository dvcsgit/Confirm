using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QS.CheckListManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public DateTime AuditDate { get; set; }

        public string AuditDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(AuditDate);
            }
        }

        public string AuditorID { get; set; }

        public string AuditorName { get; set; }

        public string Auditor
        {
            get
            {
                if (!string.IsNullOrEmpty(AuditorName))
                {
                    return string.Format("{0}/{1}", AuditorID, AuditorName);
                }
                else
                {
                    return AuditorID;
                }
            }
        }

        public string AuditorManagerID { get; set; }

        public string AuditorManagerName { get; set; }

        public string AuditorManager
        {
            get
            {
                if (!string.IsNullOrEmpty(AuditorManagerName))
                {
                    return string.Format("{0}/{1}", AuditorManagerID, AuditorManagerName);
                }
                else
                {
                    return AuditorManagerID;
                }
            }
        }

        public string FactoryUniqueID { get; set; }

        public string FactoryDescription { get; set; }

        public string Factory
        {
            get
            {
                return FactoryDescription;
            }
        }

        public string ShiftUniqueID { get; set; }

        public string ShiftDescription { get; set; }

        public string Shift
        {
            get
            {
                return ShiftDescription;
            }
        }

        public List<string> StationList { get; set; }

        public string Stations
        {
            get
            {
                if (StationList != null && StationList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var station in StationList)
                    {
                        sb.Append(station);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<CheckTypeModel> CheckTypeList { get; set; }

        public List<PhotoModel> PhotoList
        {
            get
            {
                return CheckTypeList.SelectMany(x => x.PhotoList).ToList();
            }
        }

        public DetailViewModel()
        {
            CheckTypeList = new List<CheckTypeModel>();
        }
    }
}
