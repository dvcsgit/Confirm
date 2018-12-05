using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.AbnormalForm
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public string OtherInformation { get; set; }

        public string HandlingRemark { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string Status { get; set; }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
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

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
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

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

        [Display(Name = "Calibrator", ResourceType = typeof(Resources.Resource))]
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

        public EquipmentModel Equipment { get; set; }

        public List<DetailItem> ItemList { get; set; }

        public List<STDUSEModel> STDUSEList { get; set; }

        public List<FileModel> FileList { get; set; }

        public ChangeFormModel ChangeForm { get; set; }

        public string FlowVHNO { get; set; }

        public string FlowClosedDate { get; set; }

        public string FlowClosedDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(FlowClosedDate);
            }
        }

        public string FlowFileExtension { get; set; }

        public List<LogModel> LogList { get; set; }

        public DetailViewModel()
        {
            ItemList = new List<DetailItem>();
            STDUSEList = new List<STDUSEModel>();
            FileList = new List<FileModel>();
            ChangeForm = new ChangeFormModel();
            LogList = new List<LogModel>();
        }
    }
}
