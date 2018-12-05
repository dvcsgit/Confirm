using System.Collections.Generic;
using System.Web.Mvc;
using Utility;

namespace Models.EquipmentMaintenance.AbnormalHandlingManagement
{
    public class QueryFormModel
    {
        public List<SelectListItem> TypeSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Value = "P", Text = "巡檢" },
                    new SelectListItem() { Value = "M", Text = "預防保養" }
                };
            }
        }

        public List<SelectListItem> AbnormalTypeSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Value = "1", Text = "異常" },
                    new SelectListItem() { Value = "2", Text = "注意" }
                };
            }
        }

        public List<SelectListItem> ClosedStatusSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Value = "1", Text = "已結案" },
                    new SelectListItem() { Value = "0", Text = "未結案" }
                };
            }
        } 

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
        }
    }
}
