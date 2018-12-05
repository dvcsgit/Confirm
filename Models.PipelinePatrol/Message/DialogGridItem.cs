using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Message
{
    public class DialogGridItem
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

        public DateTime? LastMessageTime { get; set; }

        public string LastMessageTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(LastMessageTime);
            }
        }
    }
}
