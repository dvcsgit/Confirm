using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MonthlyReport
{
    public class QueryParameters
    {
        public string BeginYear { get; set; }

        public string BeginMonth { get; set; }

        public DateTime BeginDate
        {
            get
            {
                return new DateTime(int.Parse(BeginYear), int.Parse(BeginMonth), 1);
            }
        }

        public string EndYear { get; set; }

        public string EndMonth { get; set; }

        public DateTime EndDate
        {
            get
            {
                return new DateTime(int.Parse(EndYear), int.Parse(EndMonth), DateTime.DaysInMonth(int.Parse(EndYear), int.Parse(EndMonth)));
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

        public QueryParameters()
        {
            BeginYear = DateTime.Today.Year.ToString();
            EndYear = DateTime.Today.Year.ToString();
        }
    }
}
