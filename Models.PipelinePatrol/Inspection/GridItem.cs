using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Inspection
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Description { get; set; }

        public string ConstructionFirmUniqueID { get; set; }

        public string ConstructionFirmName { get; set; }

        public string ConstructionFirmRemark { get; set; }

        public string ConstructionFirm
        {
            get
            {
                if (ConstructionFirmUniqueID == Define.OTHER)
                {
                    return ConstructionFirmRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(ConstructionFirmName))
                    {
                        return ConstructionFirmName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public string ConstructionTypeUniqueID { get; set; }

        public string ConstructionTypeDescription { get; set; }

        public string ConstructionTypeRemark { get; set; }

        public string ConstructionType
        {
            get
            {
                if (ConstructionTypeUniqueID == Define.OTHER)
                {
                    return ConstructionTypeRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(ConstructionTypeDescription))
                    {
                        return ConstructionTypeDescription;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

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
