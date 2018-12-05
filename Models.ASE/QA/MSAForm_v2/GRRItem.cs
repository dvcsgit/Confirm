using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
    public class GRRItem
    {
        public string Level { get; set; }

        public DateTime? Date { get; set; }

        public string DateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(Date);
            }
        }

        public List<GRRAppraiser> AppraiserList { get; set; }

        public bool IsFinished
        {
            get
            {
                return !AppraiserList.Any(x => !x.IsFinished);
            }
        }

        public decimal? Average
        {
            get
            {
                if (IsFinished)
                {
                    return AppraiserList.Average(x => x.Average.Value);
                }
                else
                {
                    return null;
                }
            }
        }

        public GRRItem()
        {
            AppraiserList = new List<GRRAppraiser>();
        }
    }
}

