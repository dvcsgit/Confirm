using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    new SelectListItem() { Text = "簽核中", Value = "1", Selected = true },
                    new SelectListItem() { Text = "退回修正", Value = string.Format("0{0}2", Define.Seperator), Selected = true },
                    new SelectListItem() { Text = "簽核完成", Value = "3", Selected = false },
                    new SelectListItem() { Text = "取消立案", Value = "4", Selected = false }
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
