using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string FullOrganizationDescription { get; set; }

        public string MaintenanceOrganization { get; set; }

        public string CheckResultUniqueID { get; set; }

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

        public CreateFormInput FormInput { get; set; }

        public Dictionary<string, OrganizationModel> EquipmentMaintenanceOrganizationList { get; set; }

        public Dictionary<string, List<RFormSubject>> RepairFormTypeSubjectList { get; set; }

        public List<FileModel> FileList { get; set; }

        public CreateFormModel()
        {
            SubjectSelectItemList = new List<SelectListItem>();
            EquipmentSelectItemList = new List<SelectListItem>();
            RepairFormTypeSelectItemList = new List<SelectListItem>();
            FormInput = new CreateFormInput();
            EquipmentMaintenanceOrganizationList = new Dictionary<string, OrganizationModel>();
            RepairFormTypeSubjectList = new Dictionary<string, List<RFormSubject>>();
            FileList = new List<FileModel>();
        }
    }
}
