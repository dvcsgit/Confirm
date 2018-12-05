using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Models.Authenticated
{
    public class PasswordFormModel
    {
        [Display(Name = "Opassword", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "OpasswordRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Opassword { get; set; }

        [Display(Name = "Npassword", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "NpasswordRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Npassword { get; set; }

        [Display(Name = "Cpassword", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "CpasswordRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        [System.ComponentModel.DataAnnotations.Compare("Npassword", ErrorMessageResourceName = "NCPasswordDiffer", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Cpassword { get; set; }
    }
}
