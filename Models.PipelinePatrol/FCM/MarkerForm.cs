using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.FCM
{
    public class MarkerForm
    {
        /// <summary>
        /// MarkerFormConsts
        /// </summary>
        public string Type { get; set; }
        public string UniqueID { get; set; }

        /// <summary>
        /// For Construct check Con
        /// </summary>
        public bool? IsClosed { get; set; }

        public string UserID { get; set; }
        
        
    }

    public class MarkerFormConsts {
        public static readonly string PipelineAbnormal = "PipelineAbnormal";
        public static readonly string Construction = "Construction";
        public static readonly string Preview = "Preview";
    }
}
