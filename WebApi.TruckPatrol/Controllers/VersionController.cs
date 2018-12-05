using DataSync.TruckPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utility;

namespace WebApi.TruckPatrol.Controllers
{
    public class VersionController : ApiController
    {
        public string Get(string AppName, Define.EnumDevice Device)
        {
            return VersionHelper.Get(AppName, Device);
        }
    }
}