using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormColumnManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string AncestorOrganizationDescription { get; set; }

        [Display(Name = "RepairFormColumnDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<string> OptionDescriptionList { get; set; }

        public string Options
        {
            get
            {
                if (OptionDescriptionList != null && OptionDescriptionList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var option in OptionDescriptionList)
                    {
                        sb.Append(option);

                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool CanDelete { get; set; }

        public DetailViewModel()
        {
            OptionDescriptionList = new List<string>();
        }
    }
}
