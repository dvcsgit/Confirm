using System.Web.Http;
using DataAccess.ASE;
using System.Collections.Generic;
using System.Net.Http;
using Utility.Models;
using System.Net;
using System.Reflection;
using System;
using Utility;

namespace WebApi.ASE.E.Controllers
{
    public class LastModifyTimeController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Get(List<string> JobList)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = LastModifyTimeHelper.Get(JobList);

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