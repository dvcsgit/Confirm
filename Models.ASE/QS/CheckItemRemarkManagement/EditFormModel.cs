using System.Collections.Generic;
using System.ComponentModel;
namespace Models.ASE.QS.CheckItemRemarkManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [DisplayName("稽核類別代號")]
        public decimal CheckTypeID { get; set; }

        [DisplayName("稽核類別英文描述")]
        public string CheckTypeEDescription { get; set; }

        [DisplayName("稽核類別中文描述")]
        public string CheckTypeCDescription { get; set; }

        [DisplayName("稽核項目代號")]
        public decimal CheckItemID { get; set; }

        [DisplayName("稽核項目英文描述")]
        public string CheckItemEDescription { get; set; }

        [DisplayName("稽核項目中文描述")]
        public string CheckItemCDescription { get; set; }

        [DisplayName("抽樣次數")]
        public decimal CheckTimes { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        public List<string> CheckItemRemarkList { get; set; }

        public List<RemarkModel> RemarkList { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            CheckItemRemarkList = new List<string>();
            RemarkList = new List<RemarkModel>();
        }
    }
}
