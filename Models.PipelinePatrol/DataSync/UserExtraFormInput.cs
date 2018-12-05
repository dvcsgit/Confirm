using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class UserExtraFormInput
    {
        public string UserID { get; set; }

        public string FCMID { get; set; }

        public string DeviceID { get; set; }
        
        public string IMEI { get; set; }
    }
}
