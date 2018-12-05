using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationApply
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }

        public ApplyStatus Status { get; set; }

        [DisplayName("廠別")]
        public string Factory { get; set; }

        [DisplayName("部門")]
        public string Department { get; set; }

        public DateTime? CreateTime { get; set; }

        [DisplayName("立案時間")]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string CreatorID { get; set; }

        public string CreatorName { get; set; }

        [DisplayName("立案人員")]
        public string Creator
        {
            get
            {
                if (!string.IsNullOrEmpty(CreatorName))
                {
                    return string.Format("{0}/{1}", CreatorID, CreatorName);
                }
                else
                {
                    return CreatorID;
                }
            }
        }

        public string FormUniqueID { get; set; }
        public string FormVHNO { get; set; }

        public List<LogModel> LogList { get; set; }

        public FormInput FormInput { get; set; }

        public List<DetailItem> ItemList { get; set; }

        public List<SelectListItem> FactorySelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                    new SelectListItem() { Text = "M(封裝、測試)", Value = "M" },
                    new SelectListItem() { Text = "A(CP福雷電)", Value = "A" }
                };
            }
        }

        public List<SelectListItem> IchiTypeSelectItemList { get; set; }

        public List<SelectListItem> IchiSelectItemList { get; set; }
        public List<IchiModel> IchiList { get; set; }

        public List<SelectListItem> CharacteristicTypeSelectItemList { get; set; }
        public List<CharacteristicTypeModel> CharacteristicTypeList { get; set; }
        
        public EditFormModel()
        {
            LogList = new List<LogModel>();
            FormInput = new FormInput();
            ItemList = new List<DetailItem>();
            IchiTypeSelectItemList = new List<SelectListItem>();
            IchiSelectItemList = new List<SelectListItem>();
            CharacteristicTypeSelectItemList = new List<SelectListItem>();
            IchiList = new List<IchiModel>();
            CharacteristicTypeList = new List<CharacteristicTypeModel>();
        }
    }
}
