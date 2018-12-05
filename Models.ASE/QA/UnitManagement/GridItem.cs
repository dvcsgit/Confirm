using System.Collections.Generic;
using System.Text;

namespace Models.ASE.QA.UnitManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<string> ToleranceUnitList { get; set; }

        public string ToleranceUnits
        {
            get
            {
                if (ToleranceUnitList != null && ToleranceUnitList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var toleranceUnit in ToleranceUnitList)
                    {
                        sb.Append(toleranceUnit);
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

        public GridItem()
        {
            ToleranceUnitList = new List<string>();
        }
    }
}
