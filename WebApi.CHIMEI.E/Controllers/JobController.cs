using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;
using WebApi.CHIMEI.E.DataAccess;

namespace WebApi.CHIMEI.E.Controllers
{
    public class JobController : ApiController
    {
        public HttpResponseMessage Get(string UserID, string CheckDate)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = SyncHelper.GetJobList(UserID, CheckDate);

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
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
            }

            return response;
        }
    }
}