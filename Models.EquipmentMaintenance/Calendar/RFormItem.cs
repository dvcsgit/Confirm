using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.Calendar
{
    public class RFormItem
    {
        public string UniqueID { get; set; }

        public string Display
        {
            get
            {
                return string.Format("【設備異常修復】[{0}]{1}({2})", VHNO, Subject, StatusDescription);
            }
        }

        public string VHNO { get; set; }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "4")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate) >= 0)
                    {
                        return "5";
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

        public string StatusDescription
        {
            get
            {
                switch (Status)
                {
                    case "0":
                        return Resources.Resource.RFormStatus_0;
                    case "1":
                        return Resources.Resource.RFormStatus_1;
                    case "2":
                        return Resources.Resource.RFormStatus_2;
                    case "3":
                        return Resources.Resource.RFormStatus_3;
                    case "4":
                        return Resources.Resource.RFormStatus_4;
                    case "5":
                        return Resources.Resource.RFormStatus_5;
                    case "6":
                        return Resources.Resource.RFormStatus_6;
                    case "7":
                        return Resources.Resource.RFormStatus_7;
                    case "8":
                        return Resources.Resource.RFormStatus_8;
                    case "9":
                        return Resources.Resource.RFormStatus_9;
                    default:
                        return "-";
                }
            }
        }

        public string Subject { get; set; }

        public DateTime EstBeginDate { get; set; }

        public DateTime EstEndDate { get; set; }

        public string Begin
        {
            get
            {
                return EstBeginDate.ToString("s");
            }
        }
        public string End
        {
            get
            {
                return EstEndDate.ToString("s");
            }
        }

        public string Color
        {
            get
            {
                if (StatusCode == "0")
                {
                    return Define.Color_Orange;
                }
                else if (StatusCode == "1")
                {
                    return Define.Color_Green;
                }
                else if (StatusCode == "2")
                {
                    return Define.Color_Orange;
                }
                else if (StatusCode == "3")
                {
                    return Define.Color_Orange;
                }
                else if (StatusCode == "4")
                {
                    return Define.Color_Blue;
                }
                else if (StatusCode == "5")
                {
                    return Define.Color_Red;
                }
                else if (StatusCode == "6")
                {
                    return Define.Color_Purple;
                }
                else if (StatusCode == "7")
                {
                    return Define.Color_Red;
                }
                else if (StatusCode == "8")
                {
                    return Define.Color_Green;
                }
                else if (StatusCode == "9")
                {
                    return Define.Color_Purple;
                }
                else
                {
                    return Define.Color_Gray;
                }
            }
        }
    }
}
