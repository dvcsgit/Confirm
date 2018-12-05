using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.ComponentModel;

namespace Models.ASE.QA.CharacteristicManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("量性")]
        public string Type { get; set; }

        [DisplayName("量測特性名稱")]
        public string Description { get; set; }

        public List<string> UnitList { get; set; }

        [DisplayName("單位")]
        public string Units
        {
            get
            {
                if (this.UnitList != null && this.UnitList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var unit in this.UnitList)
                    {
                        sb.Append(unit);

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

        public DetailViewModel()
        {
            UnitList = new List<string>();
        }
    }
}
