using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string ApplyUniqueID { get; set; }
        public string ApplyVHNO { get; set; }

        public string NotifyUniqueID { get; set; }
        public string NotifyVHNO { get; set; }

        public FormStatus Status { get; set; }

        public EquipmentModel Equipment { get; set; }

        public string JobCalibratorID { get; set; }

        public string JobCalibratorName { get; set; }

        [Display(Name = "JobUser", ResourceType = typeof(Resources.Resource))]
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

        public DateTime EstCalibrateDate { get; set; }

        public string EstCalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstCalibrateDate);
            }
        }

        public DateTime NotifyTime { get; set; }

        [Display(Name = "NotifyTime", ResourceType = typeof(Resources.Resource))]
        public string NotifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(NotifyTime);
            }
        }

        public DateTime? TakeJobTime { get; set; }

        [Display(Name = "TakeJobTime", ResourceType = typeof(Resources.Resource))]
        public string TakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
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

        public List<DetailItem> ItemList { get; set; }

        public List<LogModel> LogList { get; set; }

        public List<TakeJobLogModel> TakeJobLogList { get; set; }

        public List<StepLogModel> StepLogList { get; set; }

        public string LastStep
        {
            get
            {
                if (StepLogList != null && StepLogList.Count > 0)
                {
                    return StepLogList.OrderByDescending(x => x.Seq).First().Step;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string LastStepDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(LastStep))
                {
                    if (LastStep == "1")
                    {
                        return "收件";
                    }
                    else if (LastStep == "2")
                    {
                        return "送件";
                    }
                    else if (LastStep == "3")
                    {
                        return "回件";
                    }
                    else if (LastStep == "4")
                    {
                        return "發件";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<FileModel> FileList { get; set; }

        public List<AbnormalFormModel> AbnormalFormList { get; set; }

        public List<STDUSEModel> STDUSEList { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> CalibratorSelectItemList { get; set; }

        public List<SelectListItem> LabSelectItemList { get; set; }

        public EditFormModel()
        {
            ItemList = new List<DetailItem>();
            LogList = new List<LogModel>();
            TakeJobLogList = new List<TakeJobLogModel>();
            StepLogList = new List<StepLogModel>();
            AbnormalFormList = new List<AbnormalFormModel>();
            FileList = new List<FileModel>();
            STDUSEList = new List<STDUSEModel>();
            CalibratorSelectItemList = new List<SelectListItem>();
            LabSelectItemList = new List<SelectListItem>();
        }
    }
}
