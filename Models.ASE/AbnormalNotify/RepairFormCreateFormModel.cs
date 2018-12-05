using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.AbnormalNotify
{
    public class RepairFormCreateFormModel
    {
        public string FormUniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string FullOrganizationDescription { get; set; }

        public string MaintenanceOrganization { get; set; }

        public List<SelectListItem> SubjectSelectItemList { get; set; }

        public List<SelectListItem> EquipmentSelectItemList { get; set; }

        public List<SelectListItem> RepairFormTypeSelectItemList { get; set; }

        public RepairFormCreateFormInput FormInput { get; set; }

        public Dictionary<string, OrganizationModel> EquipmentMaintenanceOrganizationList { get; set; }

        public Dictionary<string, List<RFormSubject>> RepairFormTypeSubjectList { get; set; }

        public RepairFormCreateFormModel()
        {
            SubjectSelectItemList = new List<SelectListItem>();
            EquipmentSelectItemList = new List<SelectListItem>();
            RepairFormTypeSelectItemList = new List<SelectListItem>();
            FormInput = new RepairFormCreateFormInput();
            EquipmentMaintenanceOrganizationList = new Dictionary<string, OrganizationModel>();
            RepairFormTypeSubjectList = new Dictionary<string, List<RFormSubject>>();
        }
    }
}
