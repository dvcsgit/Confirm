using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Home
{
    public class SummaryViewModel
    {
        public int PersonalItemCount
        {
            get
            {
               return PersonalItemList.Sum(x => x.Count);
            }
        }

        public List<SummaryItem> PersonalItemList { get; set; }

        public int QueryableOrganizationItemCount
        {
            get
            {
                return QueryableOrganizationItemList.Sum(x => x.Count);
            }
        }

        public List<SummaryItem> QueryableOrganizationItemList { get; set; }

        public SummaryViewModel()
        {
            PersonalItemList = new List<SummaryItem>();
            QueryableOrganizationItemList = new List<SummaryItem>();
        }
    }
}
