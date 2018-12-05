using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_0, Value = "0", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_1, Value = "1", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_2, Value = "2", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_3, Value = "3", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_4, Value = "4", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_5, Value = "5", Selected = false },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_6, Value = "6", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_7, Value = "7", Selected = true },
                    new SelectListItem() { Text = Resources.Resource.CalibrationFormStatus_8, Value = "8", Selected = true },
                    new SelectListItem() { Text = "取消立案", Value = "9", Selected = true }
                };
            }
        }

        public List<SelectListItem> FactorySelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            FactorySelectItemList = new List<SelectListItem>();
            Parameters = new QueryParameters();
        }
    }
}
