using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationApply
{
    public class ApplyStatus
    {
        public string Status { get; set; }

        //狀態描述
        public string Display
        {
            get
            {
                switch (Status)
                { 
                    case "0":
                        return "文審退回";
                    case "1":
                        return "簽核中";
                    case "2":
                        return "退回修正";
                    case "3":
                        return "簽核完成";
                    case "4":
                        return "取消立案";
                    default:
                        return "-";
                }
            }
        }

        public string LabelClass
        {
            get
            {
                switch (Status)
                {
                    case "0":
                        return "label-danger";
                    case "1":
                        return "label-purple";
                    case "2":
                        return "label-danger";
                    case "3":
                        return "label-success";
                    case "4":
                        return "label-grey";
                    default:
                        return "";
                }
            }
        }

        public ApplyStatus(string Status)
        {
            this.Status = Status;
        }
    }
}
