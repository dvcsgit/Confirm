using DataSync.PipelinePatrol;
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
    public class LastModifyTimeController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string UserID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(UserID);
                var result = LastModifyTimeHelper.Get(UserID);
                response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result });
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