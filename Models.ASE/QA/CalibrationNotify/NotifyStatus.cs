using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationNotify
{
    public class NotifyStatus
    { 
        //狀態
        public string Status { get; set; }

        //狀態描述
        public string Display
        {
            get
            {
                if (Status == "0")
                {
                    return "文審退回";
                }
                else if (Status == "1")
                {
                    return "簽核中";
                }
                else if (Status == "2")
                {
                    return "退回修正";
                }
                else if (Status == "3")
                {
                    return "簽核完成";
                }
                else if (Status == "4")
                {
                    return "取消立案";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public NotifyStatus(string Status)
        {
            this.Status = Status;
        }
    }
}
