using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;
using DataAccess.ASE;

namespace WebApi.ASE.E.Controllers
{
    public class VerController : ApiController
    {
        public HttpResponseMessage Get(string AppName)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = VerHelper.GetLastVer(AppName);

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