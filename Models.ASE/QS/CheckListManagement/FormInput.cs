using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QS.CheckListManagement
{
    public class FormInput
    {
        public string ShiftUniqueID { get; set; }

        public string AuditDateString { get; set; }

        public DateTime AuditDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(AuditDateString).Value;
            }
        }

        public string AuditorID { get; set; }

        public string AuditorManagerID { get; set; }

        public string Stations { get; set; }

        public List<string> StationList
        {
            get
            {
                if (!string.IsNullOrEmpty(Stations))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Stations);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public string CheckResults { get; set; }

        public List<string> CheckResultStringList
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(CheckResults);
            }
        }
    }
}
