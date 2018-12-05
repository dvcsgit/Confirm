using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.ComponentModel;

namespace Models.ASE.QA.MSACharacteristicManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("MSA站別")]
        public string Station { get; set; }

        [DisplayName("MSA儀器")]
        public string Ichi { get; set; }

        [DisplayName("量測特性")]
        public string Name { get; set; }

        public List<UnitModel> UnitList { get; set; }

        [DisplayName("單位")]
        public string Units
        {
            get
            {
                if (UnitList != null && UnitList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var unit in this.UnitList)
                    {
                        sb.Append(unit.Description);

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
            UnitList = new List<UnitModel>();
        }
    }
}
