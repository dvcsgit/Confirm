using System.Web.Http;
using DataSync.EquipmentMaintenance;
using System.Net.Http;
using System.Collections.Generic;
using Utility.Models;
using System.Net;
using System;
using System.Reflection;
using Utility;

namespace WebApi.EquipmentMaintenance.Controllers
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