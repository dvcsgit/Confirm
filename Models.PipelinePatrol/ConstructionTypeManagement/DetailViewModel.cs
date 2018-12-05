using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.ConstructionTypeManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "ConstructionTypeID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ConstructionTypeDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
