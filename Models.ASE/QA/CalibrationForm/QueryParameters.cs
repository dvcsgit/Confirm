using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class QueryParameters
    {
        public string EstBeginDateString { get; set; }

        public DateTime? EstBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString);
            }
        }

        public string EstEndDateString { get; set; }

        public DateTime? EstEndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        public string ActBeginDateString { get; set; }

        public DateTime? ActBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(ActBeginDateString);
            }
        }

        public string ActEndDateString { get; set; }

        public DateTime? ActEndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(ActEndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(ActEndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string Status { get; set; }

        public List<string> StatusList
        {
            get
            {
                if (!string.IsNullOrEmpty(Status))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Status);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        [Display(Name = "CalNo", ResourceType = typeof(Resources.Resource))]
        public string CalNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [Display(Name = "EquipmentOwner", ResourceType = typeof(Resources.Resource))]
        public string OwnerID { get; set; }

        [Display(Name = "EquipmentOwnerManager", ResourceType = typeof(Resources.Resource))]
        public string OwnerManagerID { get; set; }

        [Display(Name = "PE", ResourceType = typeof(Resources.Resource))]
        public string PEID { get; set; }

        [Display(Name = "PEManager", ResourceType = typeof(Resources.Resource))]
        public string PEManagerID { get; set; }

        [DisplayName("廠別")]
        public string FactoryUniqueID { get; set; }

        [DisplayName("儀器名稱")]
        public string IchiName { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }
    }
}
