using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.EquipmentManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public string MaintenanceOrganizationID { get; set; }

        public string MaintenanceOrganizationDescription { get; set; }

        public string Extension { get; set; }

        public string Photo
        {
            get {
                if (!string.IsNullOrEmpty(Extension))
                {
                    return string.Format("{0}.{1}", UniqueID, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string MaintenanceOrganization
        {
            get
            {
                if (!string.IsNullOrEmpty(MaintenanceOrganizationDescription))
                {
                    return string.Format("{0}/{1}", MaintenanceOrganizationID, MaintenanceOrganizationDescription);
                }
                else
                {
                    return MaintenanceOrganizationID;
                }
            }
        }

        public FormInput FormInput { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public List<PartModel> PartList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            SpecList = new List<SpecModel>();
            MaterialList = new List<MaterialModel>();
            PartList = new List<PartModel>();
        }
    }
}
