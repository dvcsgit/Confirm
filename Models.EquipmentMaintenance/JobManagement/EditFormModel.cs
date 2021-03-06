﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Models.EquipmentMaintenance.JobManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public string BeginTimePickerValue
        {
            get
            {
                if (FormInput != null && !string.IsNullOrEmpty(FormInput.BeginTime))
                {
                    return FormInput.BeginTime.Substring(0, 2) + ":" + FormInput.BeginTime.Substring(2);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string EndTimePickerValue
        {
            get
            {
                if (FormInput != null && !string.IsNullOrEmpty(FormInput.EndTime))
                {
                    return FormInput.EndTime.Substring(0, 2) + ":" + FormInput.EndTime.Substring(2);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string RouteUniqueID { get; set; }

        [Display(Name = "RouteID", ResourceType = typeof(Resources.Resource))]
        public string RouteID { get; set; }

        [Display(Name = "RouteName", ResourceType = typeof(Resources.Resource))]
        public string RouteName { get; set; }

        public List<ControlPointModel> ControlPointList { get; set; }

        public List<UserModel> UserList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            ControlPointList = new List<ControlPointModel>();
            UserList = new List<UserModel>();
        }
    }
}
