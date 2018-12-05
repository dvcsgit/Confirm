using DataSync.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Utility;
using Utility.Models;
using Models.PipelinePatrol.FCM;

namespace WebApi.PipelinePatrol.Controllers
{
    /// <summary>
    /// 同步檢查
    /// TODO 之後要抽離
    /// </summary>
    public class SyncCheckGetDialogController : ApiController
    {

        [HttpPost]
        public HttpResponseMessage Post(List<string> uniqueIDList)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log("Param:" + JsonConvert.SerializeObject(uniqueIDList));

                RequestResult result = ChatHelper.GetDialog(uniqueIDList);

                if (result.IsSuccess)
                {
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
            }

            return response;
        }

        
    }


}