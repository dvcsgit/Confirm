using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.MSAStationManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("站別名稱")]
        public string Name { get; set; }
    }
}
