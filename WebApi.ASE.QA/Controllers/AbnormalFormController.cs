using DataAccess.ASE.QA;
using Models.ASE.QA.DataSync;
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
    public class AbnormalFormController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Create(AbnormalFormModel Model)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = AbnormalFormHelper.Create(Model);

                if (result.IsSuccess)
                {
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