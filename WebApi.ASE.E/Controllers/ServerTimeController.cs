﻿using System;
using System.Web.Http;
using Utility;

namespace WebApi.ASE.E.Controllers
{
    public class ServerTimeController : ApiController
    {
        public string Get()
        {
            return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now);
        }
    }
}