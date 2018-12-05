using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.ConstructionFirmManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "ConstructionFirmID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ConstructionFirmName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }
    }
}
