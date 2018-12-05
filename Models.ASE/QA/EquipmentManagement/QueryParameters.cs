using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class QueryParameters
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        [DisplayName("下次校驗日期(起)")]
        public string NextCalBeginDateString { get; set; }

        public DateTime? NextCalBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(NextCalBeginDateString);
            }
        }

        [DisplayName("下次校驗日期(迄)")]
        public string NextCalEndDateString { get; set; }

        public DateTime? NextCalEndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(NextCalEndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(NextCalEndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        [DisplayName("下次MSA日期(起)")]
        public string NextMSABeginDateString { get; set; }

        public DateTime? NextMSABeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(NextMSABeginDateString);
            }
        }

        [DisplayName("下次MSA日期(迄)")]
        public string NextMSAEndDateString { get; set; }

        public DateTime? NextMSAEndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(NextMSAEndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(NextMSAEndDateString).Value.AddDays(1);
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
                    return JsonConvert.DeserializeObject<List<string>>(Status);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        [DisplayName("校驗編號")]
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
