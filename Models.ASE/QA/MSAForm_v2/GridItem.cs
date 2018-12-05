using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
    public class GridItem
    {
        public string UniqueID { get; set; }

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

        public FormStatus Status { get; set; }

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

        public int StatusSeq
        {
            get
            {
                if (Status.Status == "4")
                {
                    return 1;
                }
                else if (Status.Status == "2")
                {
                    return 2;
                }
                else if (Status.Status == "1")
                {
                    return 3;
                }
                else if (Status.Status == "3")
                {
                    return 4;
                }
                else if (Status.Status == "5")
                {
                    return 5;
                }
                else if (Status.Status == "6")
                {
                    return 6;
                }
                else
                {
                    return 7;
                }
            }
        }

        public int Seq
        {
            get
            {
                if (Account != null)
                {
                    if ((Status.Status == "1" || Status.Status == "4") && Account.ID == MSAResponsorID)
                    {
                        return 1;
                    }
                    else if (Status.Status == "3" && Account.UserAuthGroupList.Contains("QA-Verify"))
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
    }
}
