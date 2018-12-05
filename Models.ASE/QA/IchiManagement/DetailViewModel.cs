using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.ComponentModel;

namespace Models.ASE.QA.IchiManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("類別")]
        public string Type { get; set; }

        [DisplayName("儀器名稱")]
        public string Name { get; set; }

        public List<CharacteristicModel> CharacteristicList { get; set; }

        [DisplayName("量測特性")]
        public string Characteristics
        {
            get
            {
                if (this.CharacteristicList != null && this.CharacteristicList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var characteristic in this.CharacteristicList)
                    {
                        sb.Append(string.Format("[{0}]{1}", characteristic.Type, characteristic.Description));

                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DetailViewModel()
        {
            CharacteristicList = new List<CharacteristicModel>();
        }
    }
}
