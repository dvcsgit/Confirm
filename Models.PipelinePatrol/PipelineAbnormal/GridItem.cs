﻿using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.PipelineAbnormal
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Description { get; set; }

        public string AbnormalReasonUniqueID { get; set; }

        public string AbnormalReasonDescription { get; set; }

        public string AbnormalReasonRemark { get; set; }

        public string AbnormalReason
        {
            get
            {
                if (AbnormalReasonUniqueID == Define.OTHER)
                {
                    return AbnormalReasonRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(AbnormalReasonDescription))
                    {
                        return AbnormalReasonDescription;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public string Address { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public UserModel CreateUser { get; set; }
    }
}
