using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.DataSync
{
    public class DownloadDataModel
    {
        public List<UserModel> UserList { get; set; }

        public List<FactoryModel> FactoryList { get; set; }

        public List<ShiftModel> ShiftList { get; set; }

        public List<StationModel> StationList { get; set; }

        public List<CheckTypeModel> CheckTypeList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return CheckTypeList.SelectMany(x => x.CheckItemList).ToList();
            }
        }

        public List<RemarkModel> RemarkList
        {
            get
            {
                return CheckTypeList.SelectMany(x => x.RemarkList).Distinct().ToList();
            }
        }

        public List<ResDepartmentModel> ResDepartmentList { get; set; }

        public DownloadDataModel()
        {
            UserList = new List<UserModel>();
            FactoryList = new List<FactoryModel>();
            ShiftList = new List<ShiftModel>();
            StationList = new List<StationModel>();
            CheckTypeList = new List<CheckTypeModel>();
            ResDepartmentList = new List<ResDepartmentModel>();
        }
    }
}
