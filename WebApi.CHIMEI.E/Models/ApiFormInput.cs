using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class ApiFormInput
    {
        public string UserID { get; set; }

        public string IMEI { get; set; }

        public string MacAddress { get; set; }

        public string AppVersion { get; set; }
    }
}