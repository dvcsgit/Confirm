using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.EquipmentResumeQuery
{
    public class DetailViewModel
    {
        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationFullDescription { get; set; }

        [Display(Name = "EquipmentID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "EquipmentName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public List<PartModel> PartList { get; set; }

        public List<RecordModel> RecordList { get; set; }

        public DetailViewModel()
        {
            SpecList = new List<SpecModel>();
            MaterialList = new List<MaterialModel>();
            PartList = new List<PartModel>();
            RecordList = new List<RecordModel>();
        }
    }
}
