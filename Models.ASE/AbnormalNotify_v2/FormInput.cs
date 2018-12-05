using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
{
    public class FormInput
    {
        [DisplayName("發生時間 (When)")]
        [Required(ErrorMessage = "請選擇發生日期")]
        public string OccurDateString { get; set; }

        public string OccurDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(OccurDateString);
            }
        }

        [Required(ErrorMessage = "請選擇發生時間")]
        public string OccurTime { get; set; }

        [DisplayName("聯絡人員 (Who)")]
        [Required(ErrorMessage = "請輸入聯絡人員")]
        public string Contact { get; set; }

        [DisplayName("MVPN")]
        [Required(ErrorMessage = "請輸入MPVN")]
        public string Mvpn { get; set; }

        [DisplayName("異常主旨 (What)")]
        [Required(ErrorMessage = "請輸入異常主旨")]
        public string Subject { get; set; }

        [DisplayName("地點 (Where)")]
        [Required(ErrorMessage = "請輸入地點")]
        public string Location { get; set; }

        [DisplayName("異常原因 (Why)")]
        [Required(ErrorMessage = "請輸入異常原因")]
        public string Description { get; set; }

        [DisplayName("緊急對策 (How)")]
        [Required(ErrorMessage = "請輸入緊急對策")]
        public string HandlingDescription { get; set; }

        [DisplayName("復原時間")]
        public string RecoveryDateString { get; set; }

        public string RecoveryDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(RecoveryDateString);
            }
        }

        public string RecoveryTime { get; set; }

        [DisplayName("復原說明")]
        public string RecoveryDescription { get; set; }

        [DisplayName("影響區域(產線單位)")]
        public string EffectArea { get; set; }

        [DisplayName("影響系統(FAC系統)")]
        public string EffectSystem { get; set; }

        [DisplayName("損失金額")]
        public string Cost { get; set; }

        [DisplayName("負責單位")]
        [Required(ErrorMessage = "請選擇負責單位")]
        public string ResponsibleOrganizationUniqueID { get; set; }

        [DisplayName("通知群組")]
        public string Groups { get; set; }

        public List<string> GroupList
        {
            get
            {
                if (!string.IsNullOrEmpty(Groups))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Groups);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
