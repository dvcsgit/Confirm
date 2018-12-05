using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class FormInput
    {
        [Display(Name = "CalNo", ResourceType = typeof(Resources.Resource))]
        public string CalNo { get; set; }

        public bool HaveAbnormal { get; set; }

        public string TraceableNo { get; set; }

        public string STDUSE { get; set; }

        public List<string> STDUSEList
        {
            get
            {
                if (!string.IsNullOrEmpty(STDUSE))
                {
                    return JsonConvert.DeserializeObject<List<string>>(STDUSE);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public string CalibrateDateString { get; set; }

        public DateTime? CalibrateDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(CalibrateDateString);
            }
        }

        public string LabUniqueID { get; set; }

        public string CalibratorID { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? Humidity { get; set; }
    }
}
