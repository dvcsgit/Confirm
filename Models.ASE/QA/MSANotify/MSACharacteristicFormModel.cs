using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.MSANotify
{
    public class MSACharacteristicFormModel
    {
        public string UniqueID { get; set; }

        public string Name { get; set; }

        public List<SelectListItem> UnitSelectItemList { get; set; }

        public MSACharacteristicFormModel()
        {
            UnitSelectItemList = new List<SelectListItem>();
        }
    }
}
