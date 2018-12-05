using System.Linq;
using System.Collections.Generic;

namespace Models.ASE.QA.DataSync
{
    public class DownloadDataModel
    {
        public List<JobModel> JobList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return JobList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<UserModel> UserList { get; set; }

        public List<UserModel> AllUserList
        {
            get
            {
                return JobList.Select(x => x.User).ToList().Union(UserList).Distinct().ToList();
            }
        }

        public Dictionary<string, string> LastModifyTimeList
        {
            get
            {
                return JobList.ToDictionary(x => x.UniqueID, x => x.LastModifyTime);
            }
        }

        public DownloadDataModel()
        {
            JobList = new List<JobModel>();
            UserList = new List<UserModel>();
        }
    }
}
