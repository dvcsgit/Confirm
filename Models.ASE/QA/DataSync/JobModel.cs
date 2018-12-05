using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.DataSync
{
    public class JobModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(JobDescription))
                {
                    return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", RouteID, RouteName);
                }
            }
        }

        public string Remark { get; set; }

        public int TimeMode { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public bool IsCheckBySeq { get; set; }

        public bool IsShowPrevRecord { get; set; }

        public List<ControlPointModel> ControlPointList { get; set; }

        public string LastModifyTime { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<string> STDUSEList { get; set; }

        public UserModel User { get; set; }

        public JobModel()
        {
            ControlPointList = new List<ControlPointModel>();
            STDUSEList = new List<string>();
            User = new UserModel();
        }
    }
}
