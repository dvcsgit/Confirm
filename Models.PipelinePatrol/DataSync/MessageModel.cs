using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class MessageModel
    {
        public string DialogUniqueID { get; set; }

        public int Seq { get; set; }

        public UserModel User { get; set; }

        public bool IsPhoto
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
                if (IsPhoto)
                {
                    return string.Format("{0}_{1}.{2}", DialogUniqueID, Seq, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Message { get; set; }

        public string Extension { get; set; }

        public DateTime MessageTime { get; set; }
        
        public string StrMessageTime { get; set; }

        public MessageModel()
        {
            User = new UserModel();
        }
    }
}
