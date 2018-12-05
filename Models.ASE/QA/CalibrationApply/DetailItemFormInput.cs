using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationApply
{
    public class DetailItemFormInput
    {
        [DisplayName("量測特性")]
        public string CharacteristicUniqueID { get; set; }

        public string CharacteristicRemark { get; set; }

        [DisplayName("單位")]
        public string UnitUniqueID { get; set; }

        public string UnitRemark { get; set; }

        [DisplayName("使用範圍下限")]
        public string LowerUsingRange { get; set; }

        [DisplayName("單位")]
        public string LowerUsingRangeUnitUniqueID { get; set; }

        public string LowerUsingRangeUnitRemark { get; set; }

        [DisplayName("使用範圍上限")]
        public string UpperUsingRange { get; set; }

        [DisplayName("單位")]
        public string UpperUsingRangeUnitUniqueID { get; set; }

        public string UpperUsingRangeUnitRemark { get; set; }

        public string UsingRangeToleranceSymbol { get; set; }

        [DisplayName("使用範圍允收")]
        public string UsingRangeTolerance { get; set; }

        [DisplayName("單位")]
        public string UsingRangeToleranceUnitUniqueID { get; set; }

        public string UsingRangeToleranceUnitRemark { get; set; }

        public string SubItems { get; set; }

        public List<string> SubItemList
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(SubItems);
            }
        }
    }
}
