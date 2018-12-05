using DataAccess.ASE;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.ASE.E.Controllers
{
    public class JobController : ApiController
    {
        public HttpResponseMessage Get(string UserID, string CheckDate)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(string.Format("Job/Get?UserID={0}&CheckDate={1}", UserID, CheckDate));

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