using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class WorkingHourFormInput
    {
        [Display(Name = "MaintenanceUser", ResourceType = typeof(Resources.Resource))]
        public string UserID { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "BeginDateRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "EndDateRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }

        [Display(Name = "WorkingHour", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "WorkingHourRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public double WorkingHour { get; set; }
    }
}
