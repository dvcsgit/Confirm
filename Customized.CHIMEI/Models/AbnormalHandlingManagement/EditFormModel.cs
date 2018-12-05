using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.AbnormalHandlingManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string FirstAbnormalUniqueID
        {
            get
            {
                return CheckResultList.OrderBy(x => x.CheckTime).ThenBy(x => x.CheckItemID).First().AbnormalUniqueID;
            }
        }

        public string EquipmentUniqueID { get; set; }

        public string CheckDate { get; set; }

        public string CheckDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(CheckDate);
            }
        }

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

        public bool IsClosed
        {
            get
            {
                return ClosedTime.HasValue;
            }
        }

        public DateTime? ClosedTime { get; set; }

        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public UserModel ClosedUser { get; set; }

        public string ClosedRemark { get; set; }

        public List<PhotoModel> BeforePhotoList { get; set; }

        public List<PhotoModel> AfterPhotoList { get; set; }

        public List<FileModel> FileList { get; set; }

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

        public List<CheckResultModel> CheckResultList { get; set; }

        public EditFormModel()
        {
            BeforePhotoList = new List<PhotoModel>();
            AfterPhotoList = new List<PhotoModel>();
            FileList = new List<FileModel>();
            ResponsorList = new List<UserModel>();
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
