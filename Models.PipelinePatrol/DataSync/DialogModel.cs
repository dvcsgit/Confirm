using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class DialogModel
    {
        public string UniqueID { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string PipelineAbnormalUniqueID { get; set; }

        public string InspectionUniqueID { get; set; }

        public string ConstructionUniqueID { get; set; }

        public string Extension { get; set; }

        public bool HavePhoto
        {
            get
            {
                return !string.IsNullOrEmpty(Extension);
            }
        }

        public string Photo
        {
            get
            {
                if (HavePhoto)
                {
                    return string.Format("{0}.{1}", UniqueID, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string UserID { get; set; }
    }
}
