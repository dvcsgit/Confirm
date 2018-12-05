using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.ChangeForm
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Status { get; set; }

        public string StatusDescription
        {
            get
            {
                if (Status == "1")
                {
                    return Resources.Resource.ChangeFormStatus_1;
                }
                else if (Status == "2")
                {
                    return "異動退回";
                }
                else if (Status == "3")
                {
                    return Resources.Resource.ChangeFormStatus_3;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ChangeType { get; set; }

        public string ChangeTypeDescription
        {
            get
            {
                if (ChangeType == "1")
                {
                    return Resources.Resource.ChangeFormChangeType_1;
                }
                else if (ChangeType == "2")
                {
                    return Resources.Resource.ChangeFormChangeType_2;
                }
                else if (ChangeType == "3")
                {
                    return Resources.Resource.ChangeFormChangeType_3;
                }
                else if (ChangeType == "4")
                {
                    return Resources.Resource.ChangeFormChangeType_4;
                }
                else if (ChangeType == "5")
                {
                    return Resources.Resource.ChangeFormChangeType_5;
                }
                else if (ChangeType == "6")
                {
                    return Resources.Resource.ChangeFormChangeType_6;
                }
                else if (ChangeType == "7")
                {
                    return Resources.Resource.ChangeFormChangeType_7;
                }
                else if (ChangeType == "8")
                {
                    return "免MSA";
                }
                else if (ChangeType == "9")
                {
                    return "變更(校正)週期";
                }
                else if (ChangeType == "A")
                {
                    return "變更(MSA)週期";
                }
                else if (ChangeType == "B")
                {
                    return "新增校驗";
                }
                else if (ChangeType == "C")
                {
                    return "新增MSA";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Factory { get; set; }

        public string OrganizationDescription { get; set; }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

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

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public List<LogModel> LogList { get; set; }

        public bool IsQRCoded { get; set; }

        public int StatusSeq
        {
            get
            {
                if (Status == "1")
                {
                    return 1;
                }
                else if (Status == "2")
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
                    if (LogList.Any(x => x.UserID == Account.ID))
                    {
                        return 1;
                    }
                    else if (LogList.Any(x => x.FlowSeq == 8) && Account.UserAuthGroupList.Contains("QA-Verify"))
                    {
                        return 2;
                    }
                    else
                    {
                        return 3;
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
