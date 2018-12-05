using DataSync.GuardPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApi.GuardPatrol.Controllers
{
    public class LastModifyTimeController : ApiController
    {
        [HttpGet]
        public string Get(string JobUniqueID)
        {
            return LastModifyTimeHelper.Get(JobUniqueID);
        }
    }
}