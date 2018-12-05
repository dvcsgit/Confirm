
using System.ComponentModel;

namespace Models.ASE.QS.RemarkManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("備註內容")]
        public string Description { get; set; }
    }
}
