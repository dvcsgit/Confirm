using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.DataSync
{
    public class AbnormalFormModel
    {
        public string CFormUniqueID { get; set; }

        public string Remark { get; set; }

        public List<AbnormalFormItem> ItemList { get; set; }

        public List<string> STDUSEList { get; set; }

        public AbnormalFormModel()
        {
            ItemList = new List<AbnormalFormItem>();
            STDUSEList = new List<string>();
        }
    }
}
