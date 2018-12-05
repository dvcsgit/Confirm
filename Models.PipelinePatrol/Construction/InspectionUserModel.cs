using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Construction
{
    public class InspectionUserModel
    {
        public DateTime InspectionTime { get; set; }

        public string InspectionTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(InspectionTime);
            }
        }

        public UserModel User { get; set; }

        public string Remark { get; set; }
    }
}
