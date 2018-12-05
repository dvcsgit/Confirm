using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.PipelinePatrol.Models.FCMTool
{
    public class MessageData<T>
    {
        /// <summary>
        /// limit 4K
        /// </summary>
        public T data { get; set; }

        //Optional
        public string to { get; set; }

        /// <summary>
        /// high or normal
        /// </summary>
        public string priority { get; set; }

        /// <summary>
        /// TTL range: 
        /// 0 to 2,419,200 seconds
        /// (about 28 days )
        /// </summary>
        public int time_to_live { get; set; }

        

        public List<string> registration_ids { get; set; }

        /// <summary>
        /// 塞topic
        /// </summary>
        public string condition { get; set; }


        /// <summary>
        /// 最多4個
        /// </summary>
        public string collapse_key { get; set; }
        

        /// <summary>
        /// true => 
        /// </summary>
        public bool delay_while_idle { get; set; }

        

    }
}