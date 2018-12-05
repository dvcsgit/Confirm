using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.PipelineAbnormal
{
    public class MessageModel
    {
        public UserModel User { get; set; }

        public DateTime MessageTime { get; set; }

        public string MessageTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(MessageTime);
            }
        }

        public string Message { get; set; }

        public string DialogUniqueID { get; set; }

        public int Seq { get; set; }

        public string Extension { get; set; }

        public string Photo
        {
            get
            {
                if (!string.IsNullOrEmpty(Extension))
                {
                    return string.Format("{0}_{1}.{2}", DialogUniqueID, Seq, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsPhoto
        {
            get
            {
                return !string.IsNullOrEmpty(Photo);
            }
        }
    }
}
