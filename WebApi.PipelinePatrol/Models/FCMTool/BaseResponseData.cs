using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.PipelinePatrol.Models.FCMTool
{
    /// <summary>
    /// 基本類別 要給 PushData<T> 的 自訂class 請一定要繼承 BaseResponseData
    /// 才能讓 前端 忽略處理
    /// </summary>
    public abstract class BaseResponseData
    {
        public string UserID { get; set; }
    }
}