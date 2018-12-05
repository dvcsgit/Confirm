using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class CALFormInput
    {
        public string EstCalDateString { get; set; }

        public DateTime? EstCalDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstCalDateString);
            }
        }
    }
}
