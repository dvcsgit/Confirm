using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.LabManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("實驗室名稱")]
        public string Description { get; set; }
    }
}
