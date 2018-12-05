using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSANotify
{
    public class QueryParameters
    {
        public string BeginDateString { get; set; }

        public DateTime? BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString);
            }
        }

        public string EndDateString { get; set; }

        public DateTime? EndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string Status { get; set; }

        public List<string> StatusList
        {
            get
            {
                if (!string.IsNullOrEmpty(Status))
                {
                    var statusList = new List<string>();

                    var temp = JsonConvert.DeserializeObject<List<string>>(Status);

                    foreach (var t in temp)
                    {
                        statusList.AddRange(t.Split(Define.Seperators, StringSplitOptions.None).ToList());
                    }

                    return statusList;
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [DisplayName("校驗編號")]
        public string CalNo { get; set; }

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
