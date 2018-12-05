
using System.ComponentModel;

namespace Models.ASE.QS.StationManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public string Type { get; set; }

        public string TypeDisplay
        {
            get
            {
                if (Type == "1") {
                    return "表頭";
                }
                else if (Type == "2")
                {
                    return "稽核項目";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DisplayName("受稽站別描述")]
        public string Description { get; set; }
    }
}
