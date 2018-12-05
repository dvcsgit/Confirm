using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Utility;

namespace Models.EquipmentMaintenance.StandardManagement
{
    public class FormInput
    {
        [Display(Name = "MaintenanceType", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceType { get; set; }

        [Display(Name = "StandardID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "StandardIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "StandardDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "StandardDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "IsFeelItem", ResourceType = typeof(Resources.Resource))]
        public bool IsFeelItem { get; set; }

        [Display(Name = "UpperLimit", ResourceType = typeof(Resources.Resource))]
        public string UpperLimit { get; set; }

        [Display(Name = "UpperAlertLimit", ResourceType = typeof(Resources.Resource))]
        public string UpperAlertLimit { get; set; }

        [Display(Name = "LowerAlertLimit", ResourceType = typeof(Resources.Resource))]
        public string LowerAlertLimit { get; set; }

        [Display(Name = "LowerLimit", ResourceType = typeof(Resources.Resource))]
        public string LowerLimit { get; set; }

        [Display(Name = "IsAccumulation", ResourceType = typeof(Resources.Resource))]
        public bool IsAccumulation { get; set; }

        [Display(Name = "AccumulationBase", ResourceType = typeof(Resources.Resource))]
        public string AccumulationBase { get; set; }

        [Display(Name = "Unit", ResourceType = typeof(Resources.Resource))]
        public string Unit { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public string FeelOptions { get; set; }

        public List<FeelOptionModel> FeelOptionList
        {
            get
            {
                var feelOptionList = new List<FeelOptionModel>();

                var temp = JsonConvert.DeserializeObject<List<string>>(FeelOptions);

                int seq = 1;

                foreach (var t in temp)
                {
                    string[] x = t.Split(Define.Seperators, StringSplitOptions.None);

                    feelOptionList.Add(new FeelOptionModel()
                    {
                        UniqueID = x[0],
                        Description = x[1],
                        IsAbnormal = x[2] == "Y",
                        Seq = seq
                    });

                    seq++;
                }

                return feelOptionList;
            }
        }
    }
}
