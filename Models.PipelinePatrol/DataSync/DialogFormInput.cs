using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class DialogFormInput
    {
        public string Subject { get; set; }

        public string Description { get; set; }

        public string PipelineAbnormalUniqueID { get; set; }

        public string InspectionUniqueID { get; set; }

        public string ConstructionUniqueID { get; set; }

        /// <summary>
        /// 轉單人
        /// </summary>
        public string UserID { get; set; }
    }
}
