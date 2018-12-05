using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.OverTimeReasonManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "AbnormalReasonID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "AbnormalReasonDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
