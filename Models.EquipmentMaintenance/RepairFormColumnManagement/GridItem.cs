using System.Collections.Generic;
using System.Text;
namespace Models.EquipmentMaintenance.RepairFormColumnManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

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

        public GridItem()
        {
            OptionDescriptionList = new List<string>();
        }
    }
}
