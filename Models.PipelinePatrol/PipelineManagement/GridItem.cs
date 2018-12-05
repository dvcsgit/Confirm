using Utility;
namespace Models.PipelinePatrol.PipelineManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
