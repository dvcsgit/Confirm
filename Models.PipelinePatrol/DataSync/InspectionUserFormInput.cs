﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class InspectionUserFormInput
    {
        public string InspectionUniqueID { get; set; }

        public string UserID { get; set; }

        public string Remark { get; set; }

        public DateTime InspectTime { get; set; }
    }
}