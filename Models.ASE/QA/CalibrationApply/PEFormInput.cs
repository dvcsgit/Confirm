using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationApply
{
    public class PEFormInput
    {
        public string MSAType { get; set; }

        public string MSASubType { get; set; }

        [Display(Name = "Station", ResourceType = typeof(Resources.Resource))]
        public string MSAStationUniqueID { get; set; }

        public string MSAStationRemark { get; set; }

        [Display(Name = "ICHI", ResourceType = typeof(Resources.Resource))]
        public string MSAIchiUniqueID { get; set; }

        public string MSAIchiRemark { get; set; }

        public string MSACharacteristic { get; set; }

        public List<string> MSACharacteristicList
        {
            get
            {
                var stringList = new List<string>();

                if (!string.IsNullOrEmpty(MSACharacteristic))
                {
                    stringList = JsonConvert.DeserializeObject<List<string>>(MSACharacteristic);
                }

                return stringList;
            }
        }

        [Display(Name = "VerifyComment", ResourceType = typeof(Resources.Resource))]
        public string Comment { get; set; }

        [Display(Name = "PE", ResourceType = typeof(Resources.Resource))]
        public string PEID { get; set; }

        [Display(Name = "PEManager", ResourceType = typeof(Resources.Resource))]
        public string PEManagerID { get; set; }
    }
}
