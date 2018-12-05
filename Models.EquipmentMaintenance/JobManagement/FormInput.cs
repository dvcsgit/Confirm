using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.JobManagement
{
    public class FormInput
    {
        [Display(Name = "JobDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "JobDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "IsNeedVerify", ResourceType = typeof(Resources.Resource))]
        public bool IsNeedVerify { get; set; }

        [Display(Name = "IsCheckBySeq", ResourceType = typeof(Resources.Resource))]
        public bool IsCheckBySeq { get; set; }

        [Display(Name = "IsShowPrevRecord", ResourceType = typeof(Resources.Resource))]
        public bool IsShowPrevRecord { get; set; }

        public string CycleMode { get; set; }

        public int CycleCount { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
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

         [Display(Name = "TimeMode", ResourceType = typeof(Resources.Resource))]
        public int TimeMode { get; set; }

        [Display(Name = "BeginTime", ResourceType = typeof(Resources.Resource))]
        public string BeginTime { get; set; }

        [Display(Name = "EndTime", ResourceType = typeof(Resources.Resource))]
        public string EndTime { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public FormInput()
        {
            TimeMode = 1;
            CycleCount = 1;
        }
    }
}
