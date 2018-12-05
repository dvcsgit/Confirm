using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.Inspection
{
    public class QueryParameters
    {
        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }
        
        [Display(Name = "ConstructionFirm", ResourceType = typeof(Resources.Resource))]
        public string ConstructionFirmUniqueID { get; set; }

        [Display(Name = "ConstructionType", ResourceType = typeof(Resources.Resource))]
        public string ConstructionTypeUniqueID { get; set; }
    }
}
