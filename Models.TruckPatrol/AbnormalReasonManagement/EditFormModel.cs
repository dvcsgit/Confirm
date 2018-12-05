﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.TruckPatrol.AbnormalReasonManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<HandlingMethodModel> HandlingMethodList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            HandlingMethodList = new List<HandlingMethodModel>();
        }
    }
}
