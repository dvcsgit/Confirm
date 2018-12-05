using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.GuardPatrol.RouteManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<ControlPointModel> ControlPointList { get; set; }

        public List<ManagerModel> ManagerList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            ControlPointList = new List<ControlPointModel>();
            ManagerList = new List<ManagerModel>();
        }
    }
}
