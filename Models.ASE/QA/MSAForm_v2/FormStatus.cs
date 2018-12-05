using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class FormStatus
    {
        public string Status { get; set; }

        private DateTime EstMSADate { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "1")
                {
                    if (DateTime.Compare(DateTime.Today, EstMSADate) >= 0)
                    {
                        return "2";
                    }
                    else
                    {
                        return Status;
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        public string Display
        {
            get
            {
                if (StatusCode == "1")
                {
                    return Resources.Resource.MSAFormStatus_1;
                }
                else if (StatusCode == "2")
                {
                    return Resources.Resource.MSAFormStatus_2;
                }
                else if (StatusCode == "3")
                {
                    return "簽核中";
                }
                else if (StatusCode == "4")
                {
                    return "退回修正";
                }
                else if (StatusCode == "5")
                {
                    return "已完成";
                }
                else if (StatusCode == "6")
                {
                    return "取消立案";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public FormStatus(string Status, DateTime EstMSADate)
        {
            this.Status = Status;
            this.EstMSADate = EstMSADate;
        }
    }
}
