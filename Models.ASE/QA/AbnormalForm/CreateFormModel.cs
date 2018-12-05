using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.AbnormalForm
{
    public class CreateFormModel
    {
        public string CalibrationFormUniqueID { get; set; }

        public EquipmentModel Equipment { get; set; }

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

        public FormInput FormInput { get; set; }

        public List<Models.ASE.QA.CalibrationForm.DetailItem> ItemList { get; set; }

        public List<Models.ASE.QA.CalibrationForm.STDUSEModel> STDUSEList { get; set; }

        public List<FileModel> FileList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            ItemList = new List<Models.ASE.QA.CalibrationForm.DetailItem>();
            STDUSEList = new List<CalibrationForm.STDUSEModel>();
            FileList = new List<FileModel>();
        }
    }
}
