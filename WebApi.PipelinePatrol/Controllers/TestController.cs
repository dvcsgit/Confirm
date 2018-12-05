using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Models.PipelinePatrol.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WebApi.PipelinePatrol.Controllers
{
    public class TestController : ApiController
    {
        // GET api/test
        public HttpResponseMessage Get(string arg)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            var datetime = DateTime.Now;


            string json_date = JsonConvert.SerializeObject(datetime, new IsoDateTimeConverter() { DateTimeFormat = DateFormateConsts.UI_S_yyyyMMddhhmmss });

            response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = false, NET_DATE = datetime ,JSON_DATE = json_date });
            return response;
        }


    }
}
