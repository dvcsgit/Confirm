using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class FormInput
    {
        [Display(Name = "MaintenanceJobDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "MaintenanceJobDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "NotifyDay", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "NotifyDayRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public int NotifyDay { get; set; }

        public string CycleMode { get; set; }

        public int CycleCount { get; set; }

        public string DayMode { get; set; }
        public string WeekMode { get; set; }
        public string MonthMode { get; set; }

        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public bool Sun { get; set; }
        public int? Day { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "BeginDateRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public DateTime BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString).Value;
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public DateTime? EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);
            }
        }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public FormInput()
        {
            NotifyDay = 7;
            CycleCount = 1;
        }
    }
}
