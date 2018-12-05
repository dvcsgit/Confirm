using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TankPatrol.PortManagement
{
    public class GridViewModel
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }
        public string StationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string FullOrganizationDescription { get; set; }

        public string StationDescription { get; set; }

        public string IslandUniqueID { get; set; }

        public string IslandDescription { get; set; }

        public string FullDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(IslandDescription))
                {
                    return string.Format("{0} -> {1} -> {2}", FullOrganizationDescription, StationDescription, IslandDescription);
                }
                else if (!string.IsNullOrEmpty(StationDescription))
                {
                    return string.Format("{0} -> {1}", FullOrganizationDescription, StationDescription);
                }
                else
                {
                    return FullOrganizationDescription;
                }
            }
        }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            ItemList = new List<GridItem>();
        }
    }
}
