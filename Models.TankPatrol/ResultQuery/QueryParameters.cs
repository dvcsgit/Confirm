﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TankPatrol.ResultQuery
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        public string StationUniqueID { get; set; }

        public string IslandUniqueID { get; set; }

        public string PortUniqueID { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }
    }
}
