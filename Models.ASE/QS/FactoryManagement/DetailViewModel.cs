
using System.ComponentModel;

namespace Models.ASE.QS.FactoryManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("受稽廠別描述")]
        public string Description { get; set; }
    }
}
