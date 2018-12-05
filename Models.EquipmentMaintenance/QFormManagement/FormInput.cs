using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.QFormManagement
{
    public class FormInput
    {
        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "SubjectRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "Contact", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ContactNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ContactName { get; set; }

        [Display(Name = "ContactTel", ResourceType = typeof(Resources.Resource))]
        public string ContactTel { get; set; }

        [Display(Name = "ContactEmail", ResourceType = typeof(Resources.Resource))]
        public string ContactEmail { get; set; }

        [Display(Name = "Captcha", ResourceType = typeof(Resources.Resource))]
        public string Captcha { get; set; }
    }
}
