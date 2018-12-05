using Customized.FPTC.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Customized.FPTC.Models.DispatchQuery
{
    public class QueryFormModel
    {
        public List<SelectListItem> CompanySelectItemList { get; set; }

        public List<SelectListItem> DepartmentSelectItemList { get; set; }

        public List<SelectListItem> IsCheckedSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Value = "N", Text = "未檢查" },
                    new SelectListItem() { Value = "Y", Text = "已檢查" }
                };
            }
        }

        public List<Department> DepartmentList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            CompanySelectItemList = new List<SelectListItem>()
            { 
                Define.DefaultSelectListItem(Resources.Resource.SelectAll)
            };

            DepartmentSelectItemList = new List<SelectListItem>() 
            { 
                Define.DefaultSelectListItem(Resources.Resource.SelectAll)
            };

            DepartmentList = new List<Department>();
        }
    }
}
