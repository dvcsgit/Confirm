using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.TruckPatrol.CheckItemManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<SelectListItem> CheckTypeSelectItemList { get; set; }

        public List<FeelOptionModel> FeelOptionList { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            CheckTypeSelectItemList = new List<SelectListItem>();
            FeelOptionList = new List<FeelOptionModel>();
            AbnormalReasonList = new List<AbnormalReasonModel>();
        }
    }
}
