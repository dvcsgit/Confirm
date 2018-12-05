using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class AbnormalFormModel
    {
        public string UniqueID { get; set; }

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
                else if (Status == "3")
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

        public string HandlingRemark { get; set; }

        public DateTime CreateTime { get; set; }

        [Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }
    }
}
