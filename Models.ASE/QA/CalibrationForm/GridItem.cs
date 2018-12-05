using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }

        public FormStatus Status { get; set; }

        [DisplayName("狀態")]
        public string StatusDisplay
        {
            get
            { 
                if(Status!=null)
                {
                    return Status.Display;
                }
                else
                {
                    return "-";
                }
            }
        }


       

        [DisplayName("儀校編號")]
        public string CalNo { get; set; }

        [DisplayName("預計校驗日期")]
        public string EstCalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstCalibrateDate);
            }
        }

        [DisplayName("廠別")]
        public string Factory { get; set; }

        [DisplayName("部門")]
        public string OrganizationDescription { get; set; }

        public string MachineNo { get; set; }

        [DisplayName("序號")]
        public string SerialNo { get; set; }

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

         [DisplayName("儀器名稱")]
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

        [DisplayName("廠牌")]
        public string Brand { get; set; }

        [DisplayName("型號")]
        public string Model { get; set; }
        

        public DateTime? CalibrateDate { get; set; }

        public string CalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CalibrateDate);
            }
        }

        public DateTime EstCalibrateDate { get; set; }

        

        public DateTime NotifyTime { get; set; }

        public string NotifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(NotifyTime);
            }
        }

        public DateTime? TakeJobTime { get; set; }

        public string TakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
            }
        }

        public string JobCalibratorID { get; set; }

        public string JobCalibratorName { get; set; }

        public string JobCalibrator
        {
            get
            {
                if (!string.IsNullOrEmpty(JobCalibratorName))
                {
                    return string.Format("{0}/{1}", JobCalibratorID, JobCalibratorName);
                }
                else
                {
                    return JobCalibratorID;
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

        public bool IsQRCoded { get; set; }

        public int StatusSeq {
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
                else if (Status.Status == "0")
                {
                    return 4;
                }
                else if (Status.Status == "8")
                {
                    return 5;
                }
                else if (Status.Status == "3")
                {
                    return 6;
                }
                else if (Status.Status == "6")
                {
                    return 7;
                }
                else if (Status.Status == "7")
                {
                    return 8;
                }
                else
                {
                    return 9;
                }
            }
        }

        public int Seq
        {
            get
            {
                if (Account != null)
                {
                    if ((Status.Status == "1" || Status.Status == "2" || Status.Status == "4") && Account.ID == CalibratorID)
                    {
                        return 1;
                    }
                    else if ((Status.Status == "0" || Status.Status == "8") && CalibrateUnit == "L" && Account.UserAuthGroupList.Contains("QA"))
                    {
                        return 2;
                    }
                    else if (Status.Status == "3" && Account.UserAuthGroupList.Contains("QA-Verify"))
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

        //public string LastStep { get; set; }

        //public string LastStepDescription
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(LastStep))
        //        {
        //            if (LastStep == "1")
        //            {
        //                return "收件";
        //            }
        //            else if (LastStep == "2")
        //            {
        //                return "送件";
        //            }
        //            else if (LastStep == "3")
        //            {
        //                return "回件";
        //            }
        //            else if (LastStep == "4")
        //            {
        //                return "發件";
        //            }
        //            else
        //            {
        //                return string.Empty;
        //            }
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //}
    }
}
