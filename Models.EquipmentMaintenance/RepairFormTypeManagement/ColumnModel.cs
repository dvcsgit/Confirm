using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.RepairFormTypeManagement
{
    public class ColumnModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public int Seq { get; set; }

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

        public ColumnModel()
        {
            OptionDescriptionList = new List<string>();
        }
    }
}
