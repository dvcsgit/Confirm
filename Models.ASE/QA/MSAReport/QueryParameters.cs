using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAReport
{
    public class QueryParameters
    {
        [DisplayName("MSA日期(起)")]
        public string MSABeginDateString { get; set; }

        public DateTime? MSABeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(MSABeginDateString);
            }
        }

        [DisplayName("MSA日期(迄)")]
        public string MSAEndDateString { get; set; }

        public DateTime? MSAEndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(MSAEndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(MSAEndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        [DisplayName("MSA校驗編號")]
        public string MSACalNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [DisplayName("廠別")]
        public string FactoryUniqueID { get; set; }

        [DisplayName("儀器名稱")]
        public string MSAIchi { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }
    }
}
