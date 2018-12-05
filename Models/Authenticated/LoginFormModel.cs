using System.ComponentModel.DataAnnotations;
using Resources;

namespace Models.Authenticated
{
    public class LoginFormModel
    {
        [Display(Name = "UserID", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "UserIDRequired", ErrorMessageResourceType = typeof(Resource))]
        public string UserID { get; set; }

        [Display(Name = "UserPassword", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "UserPasswordRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Password { get; set; }
    }
}
