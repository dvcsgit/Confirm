using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.ResultQuery
{
    public class JobModel
    {
        public string OrganizationDescription { get; set; }

        public string JobUniqueID { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string Description
        {
            get
            {
                return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
            }
        }

        public string CheckDate { get; set; }

        public bool HaveAbnormal
        {
            get
            {
                return ArriveRecordList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return ArriveRecordList.Any(x => x.HaveAlert);
            }
        }

        public List<ArriveRecordModel> ArriveRecordList { get; set; }

        public List<string> CheckUserList
        {
            get
            {
                return ArriveRecordList.Select(x => x.User).Distinct().OrderBy(x => x).ToList();
            }
        }

        public string CheckUsers
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var checkUser in CheckUserList)
                {
                    sb.Append(checkUser);
                    sb.Append("、");
                }

                sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }
        }

        public JobModel()
        {
            ArriveRecordList = new List<ArriveRecordModel>();
        }
    }
}
