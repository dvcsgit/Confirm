using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuardPatrol.DailyReport
{
    public class GridItem
    {
        public string CheckDate { get; set; }

        public string CheckUser { get; set; }

        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string CheckTime { get; set; }

        public string Remark { get; set; }
    }
}
