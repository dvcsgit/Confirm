using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class MSAFormInput
    {
        public string EstMSADateString { get; set; }

        public DateTime? EstMSADate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstMSADateString);
            }
        }
    }
}
