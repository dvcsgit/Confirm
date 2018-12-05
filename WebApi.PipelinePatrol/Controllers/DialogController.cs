using DataSync.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class DialogController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage NewDialog()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Form.Count != 0)
                {
                    var uniqueID = Guid.NewGuid().ToString();

                    string jsonStringValue = httpRequest.Form[0];

                    Logger.Log(jsonStringValue);

                    var FormInput = JsonConvert.DeserializeObject<DialogFormInput>(jsonStringValue);

                    var folder = string.Empty;

                    if (httpRequest.Files != null && httpRequest.Files.Count > 0)
                    {
                        var postedFile = httpRequest.Files[0];

                        folder = Path.Combine(Config.TempFolder, uniqueID);

                        Directory.CreateDirectory(folder);

                        postedFile.SaveAs(Path.Combine(folder, uniqueID + ".zip"));
                    }

                    RequestResult result = ChatHelper.NewDialog(uniqueID, FormInput, folder);

                    if (result.IsSuccess)
                    {
                        //push 
                        FCMTools fcmTools = new FCMTools();
                        var fcmRes = fcmTools.PushData<DialogModel>(result.Data as DialogModel, FCMTools.TOPIC_DIALOG);
                        Logger.Log(fcmRes);

                        response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Error != null ? result.Error.ErrorMessage : result.Message });
                    }
                }
                else
                {
                    throw new Exception("httpRequest.Form.Count < 0");
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

        [HttpGet]
        public HttpResponseMessage GetDialog(string DialogUniqueID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = ChatHelper.GetDialog(DialogUniqueID);

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

        [HttpGet]
        public HttpResponseMessage GetAllDialog()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = ChatHelper.GetDialog();

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