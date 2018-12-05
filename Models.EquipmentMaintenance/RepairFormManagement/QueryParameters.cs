using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class QueryParameters
    {
        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "RepairFormType", ResourceType = typeof(Resources.Resource))]
        public string RepairFormTypeUniqueID { get; set; }

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

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "MaintenanceBeginDate", ResourceType = typeof(Resources.Resource))]
        public string EstBeginDateString { get; set; }

        public DateTime? EstBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString);
            }
        }

        [Display(Name = "MaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        public string EstEndDateString { get; set; }

        public DateTime? EstEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString);
            }
        }

        [DisplayName("立案日期(起)")]
        public string CreateTimeBeginDateString { get; set; }

        public DateTime? CreateTimeBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(CreateTimeBeginDateString);
            }
        }

        [DisplayName("立案日期(迄)")]
        public string CreateTimeEndDateString { get; set; }

        public DateTime? CreateTimeEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(CreateTimeEndDateString);
            }
        }
    }
}
