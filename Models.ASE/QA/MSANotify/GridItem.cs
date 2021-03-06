﻿using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSANotify
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        //單號
        public string VHNO { get; set; }

        public NotifyStatus Status { get; set; }

        public string CalNo { get; set; }

        public DateTime? EstMSADate { get; set; }

        public string EstMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstMSADate);
            }
        }

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
                    return IchiRemark;
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

        public string OwnerID { get; set; }

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
                    if ((Status.Status == "0" || Status.Status == "2") && Account.ID == PEID)
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
