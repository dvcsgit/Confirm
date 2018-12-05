using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
    public class FormInput
    {
        public DateTime? MSADate { get; set; }

        public string StabilityLowerRange { get; set; }

        public string StabilityUpperRange { get; set; }

        public DateTime? StabilityPrepareDate { get; set; }

        public List<string> StabilityList { get; set; }

        public List<string> StabilityDateList { get; set; }

        public string BLPackageType { get; set; }

        public DateTime? BLDate { get; set; }

        public decimal? BiasReferenceValue { get; set; }

        public List<string> BiasList { get; set; }

        public List<string> LinearityReferenceValueList { get; set; }

        public List<string> LinearityList { get; set; }

        public List<string> GRRAppraiserList { get; set; }

        public List<string> GRRList { get; set; }

        public string GRRHPackageType { get; set; }
        public decimal? GRRHUSL { get; set; }
        public decimal? GRRHCL { get; set; }
        public decimal? GRRHLSL { get; set; }
        public string GRRHTvUsed { get; set; }

        public string GRRMPackageType { get; set; }
        public decimal? GRRMUSL { get; set; }
        public decimal? GRRMCL { get; set; }
        public decimal? GRRMLSL { get; set; }
        public string GRRMTvUsed { get; set; }

        public string GRRLPackageType { get; set; }
        public decimal? GRRLUSL { get; set; }
        public decimal? GRRLCL { get; set; }
        public decimal? GRRLLSL { get; set; }
        public string GRRLTvUsed { get; set; }

        public List<string> CountReferenceList { get; set; }

        public List<string> CountAppraiserList { get; set; }

        public List<string> CountValueList { get; set; }

        public string CountPackageType { get; set; }

        public FormInput()
        {
            StabilityList = new List<string>();
            StabilityDateList = new List<string>();
            BiasList = new List<string>();
            LinearityReferenceValueList = new List<string>();
            LinearityList = new List<string>();
            GRRAppraiserList = new List<string>();
            GRRList = new List<string>();
            CountReferenceList = new List<string>();
            CountAppraiserList = new List<string>();
            CountValueList = new List<string>();
        }
    }
}
