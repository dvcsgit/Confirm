using System.Linq;
using System.Collections.Generic;

namespace Models.ASE.DataSync
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

        public List<UserModel> UserList { get; set; }

        public UserModel User { get; set; }

        public List<EmgContactModel> EmgContactList { get; set; }

        public string LastModifyTime { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.AllCheckItemList).Distinct().ToList();
            }
        }

        public List<EquipmentSpecModel> EquipmentSpecList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.EquipmentSpecList).Distinct().ToList();
            }
        }

        public List<MaterialModel> MaterialList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.MaterialList).Distinct().ToList();
            }
        }

        public List<string> STDUSEList { get; set; }

        public JobModel()
        {
            ControlPointList = new List<ControlPointModel>();
            UserList = new List<UserModel>();
            User = new UserModel();
            EmgContactList = new List<EmgContactModel>();
            STDUSEList = new List<string>();
        }
    }
}
