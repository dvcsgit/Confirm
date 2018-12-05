using System.ComponentModel.DataAnnotations;

namespace Models.GuardPatrol.RouteManagement
{
    public class FormInput
    {
        [Display(Name = "RouteID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RouteIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "RouteName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RouteNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }
    }
}
