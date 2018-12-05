using Utility;

namespace Models.PipelinePatrol.PipelineSpecManagement
{
    public class GridItem
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string Type { get; set; }
        
        public string Description { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
