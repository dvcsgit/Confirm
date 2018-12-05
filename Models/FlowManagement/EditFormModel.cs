using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.FlowManagement
{
    public class EditFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string UniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<VerifyOrganizationModel> VerifyOrganizationList { get; set; }

        public List<FormModel> FormList { get; set; }

        public List<FlowForm> FlowFormList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            VerifyOrganizationList = new List<VerifyOrganizationModel>();
            FormList = new List<FormModel>();
            FlowFormList = new List<FlowForm>();
        }
    }
}
