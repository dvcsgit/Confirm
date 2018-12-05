using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.CalibrationNotify
{
    public class DetailItemEditFormModel
    {
        public int Seq { get; set; }

        public DetailItemFormInput FormInput { get; set; }

        public List<SelectListItem> CharacteristicSelectItemList { get; set; }

        public List<SelectListItem> UnitSelectItemList { get; set; }
        public List<CharacteristicUnitModel> UnitList { get; set; }

        public List<SelectListItem> LowerUsingRangeUnitSelectItemList { get; set; }

        public List<SelectListItem> UpperUsingRangeUnitSelectItemList { get; set; }

        public List<SelectListItem> UsingRangeToleranceSymbolSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    new SelectListItem() { Text = "±", Value = "1" },
                    new SelectListItem() { Text = "+", Value = "2" },
                    new SelectListItem() { Text = "-", Value = "3" },
                    new SelectListItem() { Text = ">", Value = "4" },
                    new SelectListItem() { Text = "<", Value = "5" },
                    new SelectListItem() { Text = "≧", Value = "6" },
                    new SelectListItem() { Text = "≦", Value = "7" }
                };
            }
        }

        public List<SelectListItem> UsingRangeToleranceUnitSelectItemList { get; set; }
        public List<ToleranceUnitModel> ToleranceUnitList { get; set; }
        
        public List<SubDetailItem> ItemList { get; set; }

        public DetailItemEditFormModel()
        {
            FormInput = new DetailItemFormInput();
            CharacteristicSelectItemList = new List<SelectListItem>();
            UnitSelectItemList = new List<SelectListItem>();
            LowerUsingRangeUnitSelectItemList = new List<SelectListItem>();
            UpperUsingRangeUnitSelectItemList = new List<SelectListItem>();
            UsingRangeToleranceUnitSelectItemList = new List<SelectListItem>();
            UnitList = new List<CharacteristicUnitModel>();
            ToleranceUnitList = new List<ToleranceUnitModel>();
            ItemList = new List<SubDetailItem>();
        }
    }
}
