﻿using System.Collections.Generic;

namespace Models.TruckPatrol.ResultQuery
{
    public class ArriveRecordModel
    {
        public string UniqueID { get;set; }
        
        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                return string.Format("{0}/{1}", UserID, UserName);
            }
        }

        public string UnRFIDReasonDescription { get; set; }

        public string UnRFIDReasonRemark { get; set; }

        public string UnRFIDReason
        {
            get
            {
                if (!string.IsNullOrEmpty(UnRFIDReasonDescription))
                {
                    return UnRFIDReasonDescription;
                }
                else
                {
                    return UnRFIDReasonRemark;
                }
            }
        }

        public List<string> PhotoList { get; set; }

        public bool HavePhoto
        {
            get
            {
                return this.PhotoList != null && this.PhotoList.Count > 0;
            }
        }

        public ArriveRecordModel()
        {
            PhotoList = new List<string>();
        }
    }
}
