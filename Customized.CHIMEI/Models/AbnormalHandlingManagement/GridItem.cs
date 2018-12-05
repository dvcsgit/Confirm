using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.AbnormalHandlingManagement
{
    public class GridItem
    {
        public string UniqueID { get; private set; }

        public string RouteUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string EquipmentDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public string CheckDate { get; set; }

        public string CheckDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(CheckDate);
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public List<UserModel> CheckUserList { get; set; }

        public string CheckUsers
        {
            get
            {
                if (CheckUserList != null && CheckUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var checkUser in CheckUserList)
                    {
                        sb.Append(checkUser.User);
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

        public DateTime? ClosedTime { get; set; }

        public bool IsClosed
        {
            get
            {
                return ClosedTime.HasValue;
            }
        }

        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public string ClosedRemark { get; set; }
        
        public UserModel ClosedUser { get; set; }

        public List<UserModel> ResponsorList { get; set; }

        public List<string> ResponsorUserIDList
        {
            get
            {
                return ResponsorList.Select(x => x.ID).ToList();
            }
        }

        public string Responsors
        {
            get
            {
                if (ResponsorList != null && ResponsorList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var responsor in ResponsorList)
                    {
                        sb.Append(responsor.User);
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

        public GridItem()
        {
            UniqueID = Guid.NewGuid().ToString();
            ClosedUser = new UserModel();
            CheckUserList = new List<UserModel>();
            ResponsorList = new List<UserModel>();
        }
    }
}
