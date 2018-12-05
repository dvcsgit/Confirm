using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class UnPatrolReasonModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public DateTime LastModifyTime { get; set; }
    }
}