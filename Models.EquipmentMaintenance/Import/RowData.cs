using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class RowData
    {
        public List<OrganizationRowData> OrganizationList { get; set; }
        public List<UserRowData> UserList { get; set; }
        public List<UnRFIDReasonRowData> UnRFIDReasonList { get; set; }
        public List<OverTimeReasonRowData> OverTimeReasonList { get; set; }
        public List<UnPatrolReasonRowData> UnPatrolReasonList { get; set; }
        public List<HandlingMethodRowData> HandlingMethodList { get; set; }
        public List<AbnormalReasonRowData> AbnormalReasonList { get; set; }
        public List<CheckItemRowData> CheckItemList { get; set; }
        public List<EquipmentRowData> EquipmentList { get; set; }
        public List<ControlPointRowData> ControlPointList { get; set; }
        public List<RouteRowData> RouteList { get; set; }

        public RowData()
        {
            OrganizationList = new List<OrganizationRowData>();
            UserList = new List<UserRowData>();
            UnRFIDReasonList = new List<UnRFIDReasonRowData>();
            OverTimeReasonList = new List<OverTimeReasonRowData>();
            UnPatrolReasonList = new List<UnPatrolReasonRowData>();
            HandlingMethodList = new List<HandlingMethodRowData>();
            AbnormalReasonList = new List<AbnormalReasonRowData>();
            CheckItemList = new List<CheckItemRowData>();
            EquipmentList = new List<EquipmentRowData>();
            ControlPointList = new List<ControlPointRowData>();
            RouteList = new List<RouteRowData>();
        }
    }
}
