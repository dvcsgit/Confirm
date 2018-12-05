using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class ImportModel
    {
        public List<Models.EquipmentMaintenance.Import.OrganizationModel> OrganizationList { get; set; }

        public List<UnRFIDReasonModel> UnRFIDReasonList { get; set; }

        public List<OverTimeReasonModel> OverTimeReasonList { get; set; }

        public List<UnPatrolReasonModel> UnPatrolReasonList { get; set; }

        public List<NoOrganizationItem> NoOrganizationItemList { get; set; }

        public bool CanImport
        {
            get
            {
                return
                    ValidOrganizationCount > 0 || 
                    ValidUserCount > 0 || 
                    ValidUnRFIDReasonCount > 0 || 
                    ValidOverTimeReasonCount > 0 || 
                    ValidUnPatrolReasonCount > 0 || 
                    ValidHandlingMethodCount > 0 || 
                    ValidAbnormalReasonCount > 0 || 
                    ValidCheckItemCount > 0 || 
                    ValidEquipmentCount > 0 || 
                    ValidControlPointCount > 0 || 
                    ValidRouteCount > 0;
            }
        }

        public bool HaveNewOrganization
        {
            get
            {
                return NewOrganizationCount > 0;
            }
        }

        public int NewOrganizationCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewOrganizationCount);
            }
        }

        public int ValidOrganizationCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidOrganizationCount);
            }
        }

        public bool HaveNewUser
        {
            get
            {
                return NewUserCount > 0;
            }
        }

        public int NewUserCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewUserCount);
            }
        }

        public int ValidUserCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidUserCount);
            }
        }

        public bool HaveNewUnRFIDReason
        {
            get
            {
                return NewUnRFIDReasonCount > 0;
            }
        }

        public int NewUnRFIDReasonCount
        {
            get
            {
                return UnRFIDReasonList.Count;
            }
        }

        public int ValidUnRFIDReasonCount
        {
            get
            {
                return UnRFIDReasonList.Count(x => !x.IsError);
            }
        }

        public bool HaveNewOverTimeReason
        {
            get
            {
                return NewOverTimeReasonCount > 0;
            }
        }

        public int NewOverTimeReasonCount
        {
            get
            {
                return OverTimeReasonList.Count;
            }
        }

        public int ValidOverTimeReasonCount
        {
            get
            {
                return OverTimeReasonList.Count(x => !x.IsError);
            }
        }

        public bool HaveNewUnPatrolReason
        {
            get
            {
                return NewUnPatrolReasonCount > 0;
            }
        }

        public int NewUnPatrolReasonCount
        {
            get
            {
                return UnPatrolReasonList.Count;
            }
        }

        public int ValidUnPatrolReasonCount
        {
            get
            {
                return UnPatrolReasonList.Count(x => !x.IsError);
            }
        }

        public bool HaveNewHandlingMethod
        {
            get
            {
                return NewHandlingMethodCount > 0;
            }
        }

        public int NewHandlingMethodCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewHandlingMethodCount);
            }
        }

        public int ValidHandlingMethodCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidHandlingMethodCount);
            }
        }

        public bool HaveNewAbnormalReason
        {
            get
            {
                return NewAbnormalReasonCount > 0;
            }
        }

        public int NewAbnormalReasonCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewAbnormalReasonCount);
            }
        }

        public int ValidAbnormalReasonCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidAbnormalReasonCount);
            }
        }

        public bool HaveNewCheckItem
        {
            get
            {
                return NewCheckItemCount > 0;
            }
        }

        public int NewCheckItemCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewCheckItemCount);
            }
        }

        public int ValidCheckItemCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidCheckItemCount);
            }
        }

        public bool HaveNewEquipment
        {
            get
            {
                return NewEquipmentCount > 0;
            }
        }

        public int NewEquipmentCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewEquipmentCount);
            }
        }

        public int ValidEquipmentCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidEquipmentCount);
            }
        }

        public bool HaveNewControlPoint
        {
            get
            {
                return NewControlPointCount > 0;
            }
        }

        public int NewControlPointCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewControlPointCount);
            }
        }

        public int ValidControlPointCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidControlPointCount);
            }
        }

        public bool HaveNewRoute
        {
            get
            {
                return NewRouteCount > 0;
            }
        }

        public int NewRouteCount
        {
            get
            {
                return OrganizationList.Sum(x => x.NewRouteCount);
            }
        }

        public int ValidRouteCount
        {
            get
            {
                return OrganizationList.Sum(x => x.ValidRouteCount);
            }
        }

        public ImportModel()
        {
            OrganizationList = new List<OrganizationModel>();
            UnRFIDReasonList = new List<UnRFIDReasonModel>();
            OverTimeReasonList = new List<OverTimeReasonModel>();
            UnPatrolReasonList = new List<UnPatrolReasonModel>();
            NoOrganizationItemList = new List<NoOrganizationItem>();
        }
    }
}
