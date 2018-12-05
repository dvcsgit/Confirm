using System;
using System.Collections.Generic;
using System.Text;
using Utility;
namespace Models.ASE.QS.CheckListManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Factory { get; set; }

        public string Shift { get; set; }

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
    }
}
