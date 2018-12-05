using System.Collections.Generic;
using System.Text;

namespace Models.ASE.AbnormalNotifyGroup
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string GroupType { get; set; }

        public string GroupTypeDisplay
        {
            get
            {
                if (GroupType == "1")
                {
                    return "異常通報";
                }
                else
                {
                    return "災損填報";
                }
            }
        }

        public string Description { get; set; }

        public bool CanDelete { get; set; }
    }
}
