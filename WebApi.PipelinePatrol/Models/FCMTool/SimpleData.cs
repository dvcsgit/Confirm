using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.PipelinePatrol.Models.FCMTool
{
    public class SimpleData : BaseResponseData
    {
        public string topic { get; set; }
        public string content { get; set; }
    }
}