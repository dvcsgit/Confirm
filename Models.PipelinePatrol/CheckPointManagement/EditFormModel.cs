using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.CheckPointManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "PipePointType", ResourceType = typeof(Resources.Resource))]
        public string PipePointType { get; set; }

        [Display(Name = "PipePointID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "PipePointName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
