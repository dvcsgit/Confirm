using DataSync.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.PipelinePatrol.Controllers
{
    public class InspectionUserController : ApiController
    {
        /// <summary>
        /// 會堪單 人員進行會堪
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Inspect()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Form.Count != 0)
                {
                    string jsonStringValue = httpRequest.Form[0];

                    Logger.Log(jsonStringValue);

                    var formInput = JsonConvert.DeserializeObject<InspectionUserFormInput>(jsonStringValue);

                    var isExist = InspectionHelper.CheckIfInspectionUser(formInput.InspectionUniqueID, formInput.UserID);
                    if(!isExist)
                    {
                        RequestResult result = InspectionHelper.Inspect(formInput);

                        if (result.IsSuccess)
                        {
                            //push 
                            //FCMTools fcmTools = new FCMTools();
                            //var fcmRes = fcmTools.PushData<string>(result.Data as string, "FormInspection");
                            //Logger.Log(fcmRes);
                            response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                            //response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true });
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Error != null ? result.Error.ErrorMessage : result.Message });
                        }
                    }
                    else
                    {
                        // TODO 之後要放到 Resource
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = "您已會勘過" });
                    }

                    
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