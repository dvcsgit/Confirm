using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.ChangeForm
{
    public class FormInput
    {
        [Display(Name = "ChangeReason", ResourceType = typeof(Resources.Resource))]
        public string Reason { get; set; }

        [Display(Name = "EquipmentOwner", ResourceType = typeof(Resources.Resource))]
        public string OwnerID { get; set; }

        [Display(Name = "EquipmentOwnerManager", ResourceType = typeof(Resources.Resource))]
        public string OwnerManagerID { get; set; }

        [Display(Name = "PE", ResourceType = typeof(Resources.Resource))]
        public string PEID { get; set; }

        [Display(Name = "PEManager", ResourceType = typeof(Resources.Resource))]
        public string PEManagerID { get; set; }

        [Display(Name = "FixFinishedDate", ResourceType = typeof(Resources.Resource))]
        public string FixFinishedDateString { get; set; }

        public DateTime? FixFinishedDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(FixFinishedDateString);
            }
        }

        [DisplayName("新校正週期")]
        public int? NewCALCycle { get; set; }

        [DisplayName("新MSA週期")]
        public int? NewMSACycle { get; set; }

        [DisplayName("校驗週期")]
        public int? CALCycle { get; set; }

        [DisplayName("MSA週期")]
        public int? MSACycle { get; set; }

        [DisplayName("下次校驗日期")]
        public string NextCALDateString { get; set; }

        public DateTime? NextCALDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(NextCALDateString);
            }
        }

        [DisplayName("下次MSA日期")]
        public string NextMSADateString { get; set; }

        public DateTime? NextMSADate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(NextMSADateString);
            }
        }

        public string Equipments { get; set; }

        public List<string> EquipmentList
        {
            get
            {
                if (!string.IsNullOrEmpty(Equipments))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Equipments).Distinct().ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
