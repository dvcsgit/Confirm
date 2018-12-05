using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.EquipmentMaintenance.QFormManagement
{
    public class CreateRepairFormModel
    {
        public string QFormUniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string FullOrganizationDescription { get; set; }

        public string MaintenanceOrganization { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentID))
                {
                    if (string.IsNullOrEmpty(PartDescription))
                    {
                        return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                    }
                    else
                    {
                        return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<SelectListItem> SubjectSelectItemList { get; set; }

        public List<SelectListItem> EquipmentSelectItemList { get; set; }

        public List<SelectListItem> RepairFormTypeSelectItemList { get; set; }

        public CreateRepairFormInput FormInput { get; set; }

        public Dictionary<string, OrganizationModel> EquipmentMaintenanceOrganizationList { get; set; }

        public Dictionary<string, List<RFormSubject>> RepairFormTypeSubjectList { get; set; }

        public CreateRepairFormModel()
        {
            SubjectSelectItemList = new List<SelectListItem>();
            EquipmentSelectItemList = new List<SelectListItem>();
            RepairFormTypeSelectItemList = new List<SelectListItem>();
            FormInput = new CreateRepairFormInput();
            EquipmentMaintenanceOrganizationList = new Dictionary<string, OrganizationModel>();
            RepairFormTypeSubjectList = new Dictionary<string, List<RFormSubject>>();
        }
    }
}
