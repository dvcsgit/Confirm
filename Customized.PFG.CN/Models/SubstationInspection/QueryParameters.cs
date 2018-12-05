using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Customized.PFG.CN.Models.SubstationInspection
{
    public  class QueryParameters
    {
        [Display(Name = "Year", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessage="請選擇年")]
        public string Year { get; set; }

        [Display(Name = "Month", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessage = "請選擇月")]
        public string Month { get; set; }
    }
}
