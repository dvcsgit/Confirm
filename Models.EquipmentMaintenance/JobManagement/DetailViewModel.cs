using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.JobManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string RouteUniqueID { get; set; }

        [Display(Name = "RouteID", ResourceType = typeof(Resources.Resource))]
        public string RouteID { get; set; }

        [Display(Name = "RouteName", ResourceType = typeof(Resources.Resource))]
        public string RouteName { get; set; }

        [Display(Name = "JobDescription", ResourceType = typeof(Resources.Resource))]
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
        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(this.BeginDate);
            }
        }

        public DateTime BeginDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(this.EndDate);
            }
        }

        public DateTime? EndDate { get; set; }

        [Display(Name = "TimeMode", ResourceType = typeof(Resources.Resource))]
        public string TimeMode { get; set; }

        [Display(Name = "BeginTime", ResourceType = typeof(Resources.Resource))]
        public string BeginTime { get; set; }

        [Display(Name = "EndTime", ResourceType = typeof(Resources.Resource))]
        public string EndTime { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public List<UserModel> UserList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            UserList = new List<UserModel>();
        }
    }
}
