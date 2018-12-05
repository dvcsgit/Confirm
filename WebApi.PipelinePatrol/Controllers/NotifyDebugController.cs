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
    /// 測試使用
    /// </summary>
    public class NotifyDebugController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Form.Count != 0)
                {
                    string jsonStringValue = httpRequest.Form[0];

                    Logger.Log(jsonStringValue);

                    var formInput = JsonConvert.DeserializeObject<ConstructionFormInput>(jsonStringValue);

                    RequestResult result = new RequestResult();

                    //response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                    //push 
                    FCMTools fcmTools = new FCMTools();
                    var fcmData = new MarkerForm()
                    {
                        UniqueID = formInput.UniqueID,
                        Type = MarkerFormConsts.Construction,
                        IsClosed = formInput.IsClosed,
                        UserID = formInput.UserID
                    };
                    var fcmRes = fcmTools.PushData<MarkerForm>(fcmData, FCMTools.TOPIC_GLOBAL);
                    Logger.Log(fcmRes);
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = fcmRes });
                }
                else
                {
                    throw new Exception("httpRequest.Form.Count < 0");
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
            }

            return response;
        }

        
    }
}