using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.AbnormalForm
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
                    return Resources.Resource.AbnormalFormStatus_1;
                }
                else if (Status == "2")
                {
                    return Resources.Resource.AbnormalFormStatus_2;
                }
                else if (Status == "3" || Status == "5" || Status == "6")
                {
                    return Resources.Resource.AbnormalFormStatus_3;
                }
                else if (Status == "4")
                {
                    return Resources.Resource.AbnormalFormStatus_4;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Factory { get; set; }

        public string OrganizationDescription { get; set; }

        public string CalNo { get; set; }

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

        public string PEID { get; set; }

        public string PEManagerID { get; set; }

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

        public string Calibrator
        {
            get
            {
                if (!string.IsNullOrEmpty(CalibratorName))
                {
                    return string.Format("{0}/{1}", CalibratorID, CalibratorName);
                }
                else
                {
                    return CalibratorID;
                }
            }
        }

        public DateTime CalibrateDate { get; set; }

        public string CalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CalibrateDate);
            }
        }

        public DateTime CreateTime { get; set; }

        public string CreateTimeTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }
    }
}
