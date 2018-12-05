using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class SigninFormInput
    {
        public string UserID { get; set; }

        public string Password { get; set; }

        public string UID { get; set; }

        public string FCMID { get; set; }

        public string DeviceID { get; set; }
        
        public string IMEI { get; set; }
    }
}
