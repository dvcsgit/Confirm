using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }


        public NotifyStatus Status { get; set; }

        [DisplayName("狀態")]
        public string StatusDisplay
        {
            get
            {
                if (Status != null)
                {
                    return Status.Display;
                }
                else
                {
                    return "-";
                }
            }
        }

         [DisplayName("儀校編號")]
        public string CalNo { get; set; }

        public DateTime? EstCalibrateDate { get; set; }

          [DisplayName("預計校驗日期")]
        public string EstCalibrateDateString {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstCalibrateDate);
            }
        }

          [DisplayName("廠別")]
        public string Factory { get; set; }

         [DisplayName("部門")]
        public string Department { get; set; }

         [DisplayName("設備負責人")]
         public string Owner
         {
             get
             {
                 if (!string.IsNullOrEmpty(OwnerName))
                 {
                     return string.Format("{0}/{1}", OwnerID, OwnerName);
                 }
                 else
                 {
                     return OwnerID;
                 }
             }
         }

         [DisplayName("序號")]
        public string SerialNo { get; set; }

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

          [DisplayName("儀器名稱")]
        public string Ichi
        {
            get
            {
                if (IchiUniqueID == Define.OTHER)
                {
                    return string.Format("({0}){1}", Resources.Resource.Other, IchiRemark);
                }
                else
                {
                    return IchiName;
                }
            }
        }

         [DisplayName("廠牌")]
        public string Brand { get; set; }

         [DisplayName("型號")]
        public string Model { get; set; }

        public DateTime CreateTime { get; set; }

         [DisplayName("立案時間")]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

         

        public string OwnerManagerID { get; set; }

        public string PEID { get; set; }

        public string PEManagerID { get; set; }

        public List<LogModel> LogList { get; set; }

        public int StatusSeq
        {
            get
            {
                if (Status.Status == "0" || Status.Status == "2")
                {
                    return 1;
                }
                else if (Status.Status == "1")
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }

        public int Seq
        {
            get
            {
                if (Account != null)
                {
                    //編輯
                    if ((Status.Status == "0" || Status.Status == "2") && Account.ID == OwnerID)
                    {
                        return 1;
                    }
                    //簽核
                    else if (Status.Status == "1" && LogList.Any(x => x.UserID == Account.ID))
                    {
                        return 2;
                    }
                    //簽核
                    else if (Status.Status == "1" && LogList.Any(x => x.FlowSeq == 5) && Account.UserAuthGroupList.Contains("QA-Verify"))
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public Account Account { get; set; }

        public GridItem()
        {
            LogList = new List<LogModel>();
        }
    }
}
