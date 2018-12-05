using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.DailyReport
{
    public class QueryParameters
    {
        public string BeginDateString { get; set; }

        public DateTime BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString).Value;
            }
        }

        public string EndDateString { get; set; }

        public DateTime EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString).Value;
            }
        }

        [DisplayName("校驗編號")]
        public string CalNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [DisplayName("廠別")]
        public string FactoryUniqueID { get; set; }

        [DisplayName("儀器名稱")]
        public string IchiName { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }
    }
}
