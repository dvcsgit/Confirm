using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSANotify
{
    public class QAFormInput
    {
        [DisplayName("簽核意見")]
        public string Comment { get; set; }

        public DateTime? EstMSADate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstMSADateString);
            }
        }

       [DisplayName("預計MSA日期")]
        public string EstMSADateString { get; set; }
    }
}
