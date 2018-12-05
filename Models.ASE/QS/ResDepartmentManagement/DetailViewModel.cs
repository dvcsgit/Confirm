
using System.ComponentModel;

namespace Models.ASE.QS.ResDepartmentManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("負責部門描述")]
        public string Description { get; set; }
    }
}
