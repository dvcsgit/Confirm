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
    public class UserSigninController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Signin(SigninFormInput Model)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(JsonConvert.SerializeObject(Model));

                RequestResult result = UserHelper.Signin(Model);

                if (result.IsSuccess)
                {
                    //push 
                    var extra_result = UserHelper.UpdateUserExtra(new UserExtraFormInput { 
                    
                        UserID = Model.UserID,
                        FCMID = Model.FCMID,
                        DeviceID = Model.DeviceID,
                        IMEI = Model.IMEI
                    });

                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
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