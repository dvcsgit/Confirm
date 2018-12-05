
using System.ComponentModel;

namespace Models.ASE.QS.ShiftManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("受稽班別描述")]
        public string Description { get; set; }
    }
}
