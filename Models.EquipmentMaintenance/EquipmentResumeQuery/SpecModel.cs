﻿using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.EquipmentResumeQuery
{
    public class SpecModel
    {
        public string Description { get; set; }

        public List<EquipmentSpecOption> OptionList { get; set; }

        public string OptionUniqueID { get; set; }

        public string OptionValue
        {
            get
            {
                var option = OptionList.FirstOrDefault(x => x.UniqueID == OptionUniqueID);

                if (option != null)
                {
                    return option.Description;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Value { get; set; }

        public SpecModel()
        {
            OptionList = new List<EquipmentSpecOption>();
        }
    }
}
