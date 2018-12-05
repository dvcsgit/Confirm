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
    public class ConstructionController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage New()
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

                    var uniqueID = Guid.NewGuid().ToString();

                    if (formInput.IsClosed)
                    {
                        uniqueID = formInput.UniqueID;
                    }

                    var folder = string.Empty;

                    if (httpRequest.Files != null && httpRequest.Files.Count > 0)
                    {
                        var postedFile = httpRequest.Files[0];

                        folder = Path.Combine(Config.TempFolder, uniqueID);

                        if (Directory.Exists(folder))
                        {
                            Directory.Delete(folder, true);
                        }

                        Directory.CreateDirectory(folder);

                        postedFile.SaveAs(Path.Combine(folder, uniqueID + ".zip"));
                    }

                    RequestResult result = new RequestResult();

                    if (!formInput.IsClosed)
                    {
                        // 新開單
                        result = ConstructionHelper.Create(uniqueID, formInput, folder);
                    }
                    else
                    {
                        // 結案
                        result = ConstructionHelper.Closed(uniqueID, formInput, folder);
                    }

                    if (result.IsSuccess)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                        //push 
                        FCMTools fcmTools = new FCMTools();
                        var fcmData = new MarkerForm() { 
                            UniqueID = result.Data as string,
                            Type = MarkerFormConsts.Construction,
                            IsClosed = formInput.IsClosed,
                            UserID = formInput.UserID
                        };
                        var fcmRes = fcmTools.PushData<MarkerForm>(fcmData, FCMTools.TOPIC_FORM);
                        Logger.Log(fcmRes);
                        response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Error.ErrorMessage });
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

        [HttpGet]
        public HttpResponseMessage Get(string UniqueID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(UniqueID);

                RequestResult result = ConstructionHelper.Get(UniqueID);

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