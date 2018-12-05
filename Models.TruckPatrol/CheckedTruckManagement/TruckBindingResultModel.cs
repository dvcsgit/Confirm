using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TruckPatrol.CheckedTruckManagement
{
    public class TruckBindingResultModel
    {
        [DisplayName("異常")]
        public string Abnormal
        {
            get
            {
                if (HaveAbnormal)
                {
                    return "異常";
                }
                else if (HaveAlert)
                {
                    return "注意";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string BindingUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string FirstTruckUniqueID { get; set; }

        [DisplayName("車頭")]
        public string FirstTruckNo { get; set; }

        public string SecondTruckUniqueID { get; set; }

        [DisplayName("尾車")]
        public string SecondTruckNo { get; set; }

        [DisplayName("檢查日期")]
        public string CheckDate { get; set; }

        [DisplayName("檢查人員")]
        public string CheckUser { get; set; }

        [DisplayName("完成率")]
        public string CompleteRate { get; set; }

        public string LabelClass { get; set; }

        [DisplayName("作業時間")]
        public string TimeSpan { get; set; }

        

        public bool HaveAbnormal { get; set; }

        public bool HaveAlert { get; set; }

        public DateTime? KeyTime { get; set; }

        [DisplayName("分發鑰匙時間")]
        public string KeyTimeString {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(KeyTime);
            }
        }

        public UserModel KeyUser { get; set; }

        [DisplayName("分發鑰匙人員")]
        public string KeyUserDisplay
        {
            get
            {
                if (KeyUser != null)
                {
                    return KeyUser.User;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
