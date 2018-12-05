using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.EquipmentMaintenance.AbnormalHandlingManagement
{
    public class RepairFormCreateFormModel
    {
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

        public List<SelectListItem> RepairFormTypeSelectItemList { get; set; }

        public RepairFormCreateFormInput FormInput { get; set; }

        public Dictionary<string, List<RFormSubject>> RepairFormTypeSubjectList { get; set; }

        public string AbnormalUniqueID { get; set; }

        public RepairFormCreateFormModel()
        {
            SubjectSelectItemList = new List<SelectListItem>();
            RepairFormTypeSelectItemList = new List<SelectListItem>();
            FormInput = new RepairFormCreateFormInput();
            RepairFormTypeSubjectList = new Dictionary<string, List<RFormSubject>>();
        }
    }
}
