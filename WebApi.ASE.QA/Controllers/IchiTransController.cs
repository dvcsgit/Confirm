using DataAccess.ASE;
using DataAccess.ASE.QA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.ASE.QA.Controllers
{
    public class IchiTransController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Trans(string CalNo, int Step, string OwnerID, string QAID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = IchiTransHelper.Trans(CalNo, Step, OwnerID, QAID);

                if (result.IsSuccess)
                {
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Message = result.Message });
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = false, Message = result.Message });
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