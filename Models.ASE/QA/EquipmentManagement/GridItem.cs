using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class GridItem
    {
        [DisplayName("儀校編號")]
        public string CalNoDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(CalNo))
                {
                    return CalNo;
                }
                else
                {
                    return MSACalNo;
                }
            }
        }

        public string UniqueID { get; set; }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "1")
                {
                    if (IsDelay)
                    {
                        return "2";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        [DisplayName("狀態")]
        public string StatusDescription
        {
            get
            {
                switch (StatusCode)
                {
                    case "1":
                        return "正常";
                    case "2":
                        return "逾期";
                    case "3":
                        return "免校驗";
                    case "4":
                        return "遺失";
                    case "5":
                        return "報廢";
                    case "6":
                        return "維修中";
                    case "7":
                        return "庫存";
                    default:
                        return "-";
                }
            }
        }

        public bool CAL { get; set; }

        public bool MSA { get; set; }

        //校驗編號
        
        public string CalNo { get; set; }

        public string MSACalNo { get; set; }

      

        [DisplayName("廠別")]
        public string Factory { get; set; }

        [DisplayName("部門")]
        public string OrganizationDescription { get; set; }

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

        

        [DisplayName("序號")]
        public string SerialNo { get; set; }

        [DisplayName("儀器名稱")]
        public string IchiDisplay
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

        [DisplayName("免校/校驗/MSA")]
        public string CALMSADisplay
        {
            get
            {
                if (CAL && MSA)
                {
                    return "校驗/MSA";
                }
                else if(CAL && !MSA)
                {
                return "校驗";
                }
                else if (!CAL && MSA)
                {
                    return "MSA";
                }
                else
                {
                    return "免校";
                }
            }
        }
          [DisplayName("廠牌")]
        public string Brand { get; set; }
          [DisplayName("型號")]
        public string Model { get; set; }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }
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

        public string OwnerManagerID { get; set; }

        public string OwnerManagerName { get; set; }
        [DisplayName("設備負責人主管")]
        public string OwnerManager
        {
            get
            {
                if (!string.IsNullOrEmpty(OwnerManagerName))
                {
                    return string.Format("{0}/{1}", OwnerManagerID, OwnerManagerName);
                }
                else
                {
                    return OwnerManagerID;
                }
            }
        }

        public string PEID { get; set; }

        public string PEName { get; set; }
        [DisplayName("製程負責人")]
        public string PE
        {
            get
            {
                if (!string.IsNullOrEmpty(PEName))
                {
                    return string.Format("{0}/{1}", PEID, PEName);
                }
                else
                {
                    return PEID;
                }
            }
        }

        public string PEManagerID { get; set; }

        public string PEManagerName { get; set; }
        [DisplayName("製程負責人主管")]
        public string PEManager
        {
            get
            {
                if (!string.IsNullOrEmpty(PEManagerName))
                {
                    return string.Format("{0}/{1}", PEManagerID, PEManagerName);
                }
                else
                {
                    return PEManagerID;
                }
            }
        }

        public DateTime? LastCalDate { get; set; }
        [DisplayName("上次校驗日期")]
        public string LastCalDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(LastCalDate);
            }
        }

        public DateTime? NextCalDate { get; set; }
        [DisplayName("下次校驗日期")]
        public string NextCalDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NextCalDate);
            }
        }

        public DateTime? LastMSADate { get; set; }
        [DisplayName("上次MSA日期")]
        public string LastMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(LastMSADate);
            }
        }

        public DateTime? NextMSADate { get; set; }
        [DisplayName("下次MSA日期")]
        public string NextMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NextMSADate);
            }
        }

        public bool IsDelay { get; set; }
    }
}
