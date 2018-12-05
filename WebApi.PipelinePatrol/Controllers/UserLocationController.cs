using DataSync.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.PipelinePatrol.Controllers
{
    public class UserLocationController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Update(LocationModel Model)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(JsonConvert.SerializeObject(Model));

                RequestResult result = UserHelper.UpdateLocation(Model);

                if (result.IsSuccess)
                {
                    FCMTools fcmTools = new FCMTools();
                    var fcmRes = fcmTools.PushData<LocationModel>(Model as LocationModel, FCMTools.TOPIC_USER);
                    Logger.Log(fcmRes);
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true });
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
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
        public HttpResponseMessage Disconnect(string UserID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(UserID);

                RequestResult result = UserHelper.Disconnect(UserID);

                if (result.IsSuccess)
                {
                    FCMTools fcmTools = new FCMTools();
                    var fcmRes = fcmTools.PushData<string>(UserID as string, FCMTools.TOPIC_USER);
                    Logger.Log(fcmRes);
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true });
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
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