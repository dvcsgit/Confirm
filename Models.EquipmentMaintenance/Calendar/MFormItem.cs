using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.Calendar
{
    public class MFormItem
    {
        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "1")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate) > 0)
                    {
                        return "2";
                    }
                    else
                    {
                        return "1";
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
                switch (StatusCode)
                {
                    case "0":
                        return Resources.Resource.MFormStatus_0;
                    case "1":
                        return Resources.Resource.MFormStatus_1;
                    case "2":
                        return Resources.Resource.MFormStatus_2;
                    case "3":
                        return Resources.Resource.MFormStatus_3;
                    case "4":
                        return Resources.Resource.MFormStatus_4;
                    case "5":
                        return Resources.Resource.MFormStatus_5;
                    case "6":
                        return Resources.Resource.MFormStatus_6;
                    default:
                        return "-";
                }
            }
        }

        public string VHNO { get; set; }

        public DateTime EstBeginDate { get; set; }

        public DateTime EstEndDate { get; set; }
    }
}
