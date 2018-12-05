using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class StepLogCreateFormModel
    {
        public string CalibrateUnit { get; set; }

        public string Step { get; set; }

        public string StepDescription
        {
            get
            {
                if (Step == "1")
                {
                    return "收件";
                }
                else if (Step == "2")
                {
                    return "送件";
                }
                else if (Step == "3")
                {
                    return "回件";
                }
                else if (Step == "4")
                {
                    return "取件";
                }
                else if (Step == "5")
                {
                    return "到廠校驗";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public StepLogFormInput FormInput { get; set; }

        public List<SelectListItem> HourSelectItemList { get; set; }

        public List<SelectListItem> MinuteSelectItemList { get; set; }

        public List<SelectListItem> QASelectItemList { get; set; }

        public StepLogCreateFormModel()
        {
            FormInput = new StepLogFormInput();
        }
    }
}
