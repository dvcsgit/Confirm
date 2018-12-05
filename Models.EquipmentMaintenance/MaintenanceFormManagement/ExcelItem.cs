using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class ExcelItem
    {
        [DisplayName("組織")]
        public string OrganizationDescription { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }

        [DisplayName("狀態")]
        public string Status { get; set; }

        [DisplayName("主旨")]
        public string Subject { get; set; }

        [DisplayName("設備")]
        public string Equipment { get; set; }

        [DisplayName("保養週期(起)")]
        public string CycleBeginDate { get; set; }

        [DisplayName("保養週期(迄)")]
        public string CycleEndDate { get; set; }

        [DisplayName("預計保養日期(起)")]
        public string EstBeginDate { get; set; }

        [DisplayName("預計保養日期(迄)")]
        public string EstEndDate { get; set; }

        [DisplayName("實際保養日期(起)")]
        public string BeginDate { get; set; }

        [DisplayName("實際保養日期(迄)")]
        public string EndDate { get; set; }

        [DisplayName("接案人員")]
        public string TakeJobUser { get; set; }

        [DisplayName("接案時間")]
        public string TakeJobTime { get; set; }

        [DisplayName("保養人員")]
        public string MaintenanceUser { get; set; }

        [DisplayName("保養基準")]
        public string Standard { get; set; }

        [DisplayName("結果")]
        public string Result { get; set; }

        [DisplayName("異常")]
        public string IsAlert { get; set; }

        [DisplayName("注意")]
        public string IsAbnormal { get; set; }

        [DisplayName("下限值")]
        public double? LowerLimit { get; set; }

        [DisplayName("下限警戒值")]
        public double? LowerAlertLimit { get; set; }

        [DisplayName("上限警戒值")]
        public double? UpperAlertLimit { get; set; }

        [DisplayName("上限值")]
        public double? UpperLimit { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }
    }
}
