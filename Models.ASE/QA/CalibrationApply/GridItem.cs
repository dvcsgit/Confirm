using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationApply
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        //單號
        public string VHNO { get; set; }

        public ApplyStatus Status { get; set; }

        public string Factory { get; set; }

        public string Department { get; set; }

        public string SerialNo { get; set; }

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

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

        public string Brand { get; set; }

        public string Model { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string CreatorID { get; set; }

        public string CreatorName { get; set; }

        public string Creator
        {
            get
            {
                if (!string.IsNullOrEmpty(CreatorName))
                {
                    return string.Format("{0}/{1}", CreatorID, CreatorName);
                }
                else
                {
                    return CreatorID;
                }
            }
        }

        public string OwnerID { get; set; }

        public string OwnerManagerID { get; set; }

        public string PEID { get; set; }

        public string PEManagerID { get; set; }

        public List<LogModel> LogList { get; set; }

        public bool MSA { get; set; }

        public bool CAL { get; set; }

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
                    //其他
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
