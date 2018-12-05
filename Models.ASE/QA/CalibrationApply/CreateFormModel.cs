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
    public class CreateFormModel
    {
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
        
        public CreateFormModel()
        {
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
