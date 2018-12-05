using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.ResultQuery
{
    public class TruckBindingResultModel
    {
        public string BindingUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string FirstTruckUniqueID { get; set; }

        public string FirstTruckNo { get; set; }

        public string SecondTruckUniqueID { get; set; }

        public string SecondTruckNo { get; set; }

        public string CheckDate { get; set; }

        public string CheckUser { get; set; }

        public string CompleteRate { get; set; }

        public string LabelClass { get; set; }

        public string TimeSpan { get; set; }

        public bool HaveAbnormal { get; set; }

        public bool HaveAlert { get; set; }
    }
}
