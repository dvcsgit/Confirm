using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.ResultVerify
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string Description { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public bool HaveAbnormal { get; set; }

        public bool HaveAlert { get; set; }

        public string CompleteRate { get; set; }

        public string CompleteRateLabelClass { get; set; }

        public string TimeSpan { get; set; }

        public string CheckUsers { get; set; }

        public string ArriveStatus { get; set; }

        public string ArriveStatusLabelClass { get; set; }

        public bool IsClosed { get; set; }

        public List<UserModel> CurrentVerifyUserList { get; set; }

        public List<string> CurrentVerifyUserIDList
        {
            get
            {
                return CurrentVerifyUserList.Select(x => x.ID).ToList();
            }
        }

        public string CurrentVerifyUser
        {
            get
            {
                if (CurrentVerifyUserList != null && CurrentVerifyUserList.Count>0)
                {
                    var sb = new StringBuilder();

                    foreach (var user in CurrentVerifyUserList)
                    {
                        sb.Append(user.User);
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

        //廢除
        public string CurrentVerifyUserID { get; set; }
        public string CurrentVerifyUserName { get; set; }

        public GridItem()
        {
            CurrentVerifyUserList = new List<UserModel>();
        }
    }
}
