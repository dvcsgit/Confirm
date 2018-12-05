using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Dashboard
{
    public class InspectionViewModel
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

        public double LNG { get; set; }

        public double LAT { get; set; }

        public bool IsInspected { get; set; }
    }
}
