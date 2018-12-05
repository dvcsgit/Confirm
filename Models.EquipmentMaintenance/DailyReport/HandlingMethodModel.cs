using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.DailyReport
{
    public class HandlingMethodModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string HandlingMethod
        {
            get
            {
                if (string.IsNullOrEmpty(Remark))
                {
                    return Description;
                }
                else
                {
                    return string.Format("{0}({1})", Description, Remark);
                }
            }
        }
    }
}
