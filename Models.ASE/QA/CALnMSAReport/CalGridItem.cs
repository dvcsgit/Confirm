using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CALnMSAReport
{
    public class CalGridItem
    {
        public string UniqueID { get; set; }
        public string CalibrationApplyUniqueID { get; set; }
        
        public bool IsAbnormal { get; set; }

        public string VHNO { get; set; }

        public CalFormStatus Status { get; set; }

        public string Factory { get; set; }

        public string OrganizationDescription { get; set; }

        public string CalNo { get; set; }

        public string MachineNo { get; set; }

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

        public string CalibrateType { get; set; }

        public string CalibrateTypeDisplay
        {
            get
            {
                if (CalibrateType == "IF")
                {
                    return "內校(現場)";
                }
                else if (CalibrateType == "IL")
                {
                    return "內校(實驗室)";
                }
                else if (CalibrateType == "EF")
                {
                    return "外校(現場)";
                }
                else if (CalibrateType == "EL")
                {
                    return "外校(實驗室)";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string CalibrateUnit { get; set; }

        public string CalibrateUnitDisplay
        {
            get
            {
                if (CalibrateUnit == "F")
                {
                    return "現場";
                }
                else if (CalibrateUnit == "L")
                {
                    return "實驗室";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Model { get; set; }

        public string Brand { get; set; }

        public DateTime? CalibrateDate { get; set; }

        public string CalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CalibrateDate);
            }
        }

        public DateTime EstCalibrateDate { get; set; }

        public string EstCalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstCalibrateDate);
            }
        }

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

        public string ResponsorID { get; set; }

        public string ResponsorName { get; set; }

        public string Responsor
        {
            get
            {
                if (!string.IsNullOrEmpty(ResponsorName))
                {
                    return string.Format("{0}/{1}", ResponsorID, ResponsorName);
                }
                else
                {
                    return ResponsorID;
                }
            }
        }

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

        public string LabDescription { get; set; }

        public string Calibrator
        {
            get
            {
                if (CalibrateType == "EF" || CalibrateType == "EL")
                {
                    return LabDescription;
                }
                else
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
        }
    }
}
