using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.Message
{
    public class DialogModel
    {
        public string UniqueID { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public int CurrentSeq
        {
            get
            {
                if (MessageList != null && MessageList.Count > 0)
                {
                    return MessageList.Max(x => x.Seq);
                }
                else
                {
                    return 0;
                }
            }
        }

        public List<MessageModel> MessageList { get; set; }

        public DialogModel()
        {
            MessageList = new List<MessageModel>();
        }
    }
}
