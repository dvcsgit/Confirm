using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string Status { get; set; }

        public List<string> StatusList
        {
            get
            {
                if (!string.IsNullOrEmpty(Status))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Status);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string CycleBeginDateString { get; set; }

        public DateTime? CycleBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(CycleBeginDateString);
            }
        }

        public string CycleEndDateString { get; set; }

        public DateTime? CycleEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(CycleEndDateString);
            }
        }

        public string EstBeginDateString { get; set; }

        public DateTime? EstBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString);
            }
        }

        public string EstEndDateString { get; set; }

        public DateTime? EstEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString);
            }
        }
    }
}
