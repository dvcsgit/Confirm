using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.MSAIchiManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("MSA站別")]
        public string Station { get; set; }

        [DisplayName("儀器代號")]
        public string ID { get; set; }

        [DisplayName("儀器名稱")]
        public string Name { get; set; }
    }
}
