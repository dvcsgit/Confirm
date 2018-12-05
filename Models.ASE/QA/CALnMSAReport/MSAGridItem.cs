using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CALnMSAReport
{
    public class MSAGridItem
    {
        public string UniqueID { get; set; }

        public bool IsNew { get; set; }

        public string VHNO { get; set; }

        public string Type { get; set; }

        public string SubType { get; set; }

        public string TypeDisplay
        {
            get
            {
                if (Type == "1")
                {
                    if (SubType == "1")
                    {
                        return "計量(全距平均法)";
                    }
                    else if (SubType == "2")
                    {
                        return "計量(ANOVA)";
                    }
                    else
                    {
                        return "計量";
                    }
                }
                else if (Type == "2")
                {
                    return "計數";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public MSAFormStatus Status { get; set; }

        public string Factory { get; set; }

        public string OrganizationDescription { get; set; }

        public string CalNo { get; set; }

        public DateTime EstMSADate { get; set; }

        public string EstMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstMSADate);
            }
        }

        public DateTime? MSADate { get; set; }

        public string MSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(MSADate);
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

        public string MSAResponsorID { get; set; }

        public string MSAResponsorName { get; set; }

        public string MSAResponsor
        {
            get
            {
                if (!string.IsNullOrEmpty(MSAResponsorName))
                {
                    return string.Format("{0}/{1}", MSAResponsorID, MSAResponsorName);
                }
                else
                {
                    return MSAResponsorID;
                }
            }
        }

        public string PEID { get; set; }

        public string PEName { get; set; }

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

        public string Station { get; set; }

        public string MSAIchi { get; set; }

        public string MSALowerRange { get; set; }

        public string MSAUpperRange { get; set; }

        public string MSARange
        {
            get
            {
                if (!string.IsNullOrEmpty(MSALowerRange) && !string.IsNullOrEmpty(MSAUpperRange))
                {
                    return string.Format("{0}~{1}", MSALowerRange, MSAUpperRange);
                }
                else if (!string.IsNullOrEmpty(MSALowerRange) && string.IsNullOrEmpty(MSAUpperRange))
                {
                    return string.Format(">{0}", MSALowerRange);
                }
                else if (string.IsNullOrEmpty(MSALowerRange) && !string.IsNullOrEmpty(MSAUpperRange))
                {
                    return string.Format("<{0}", MSAUpperRange);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string MSACharacteristic { get; set; }
    }
}
