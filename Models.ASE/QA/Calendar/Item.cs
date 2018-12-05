using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.Calendar
{
    public class Item
    {
        public string CalNo { get; set; }

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

        public string Ichi
        {
            get
            {
                if (IchiUniqueID == Define.OTHER)
                {
                    return IchiRemark;
                }
                else
                {
                    return IchiName;
                }
            }
        }

        public string Display
        {
            get
            {
                //if (!string.IsNullOrEmpty(StatusDescription))
                //{
                //    return string.Format("[{0}]{1}({2})", CalNo, Ichi, StatusDescription);
                //}
                //else
                //{
                //    return string.Format("[{0}]{1}", CalNo, Ichi);
                //}

                if (!string.IsNullOrEmpty(StatusDescription))
                {
                    return string.Format("[{0}]({1})", CalNo, StatusDescription);
                }
                else
                {
                    return CalNo;
                }
            }
        }

        public string StatusDescription { get; set; }

        public string Color { get; set; }

        public DateTime? EstDate { get; set; }

        public string EstDateString
        {
            get
            {
                return EstDate.Value.ToString("s");
            }
        }
    }
}
