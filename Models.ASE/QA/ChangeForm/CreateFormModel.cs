using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.ChangeForm
{
    public class CreateFormModel
    {
        public string AbnormalFormUniqueID { get; set; }

        public string ChangeType { get; set; }

        [Display(Name = "ChangeType", ResourceType = typeof(Resources.Resource))]
        public string ChangeTypeDescription
        {
            get
            {
                if (ChangeType == "1") {
                    return Resources.Resource.ChangeFormChangeType_1;
                }
                else if (ChangeType == "2")
                {
                    return Resources.Resource.ChangeFormChangeType_2;
                }
                else if (ChangeType == "3")
                {
                    return Resources.Resource.ChangeFormChangeType_3;
                }
                else if (ChangeType == "4")
                {
                    return Resources.Resource.ChangeFormChangeType_4;
                }
                else if (ChangeType == "5")
                {
                    return Resources.Resource.ChangeFormChangeType_5;
                }
                else if (ChangeType == "6")
                {
                    return Resources.Resource.ChangeFormChangeType_6;
                }
                else if (ChangeType == "7")
                {
                    return Resources.Resource.ChangeFormChangeType_7;
                }
                else if (ChangeType == "8")
                {
                    return "免MSA";
                }
                else if (ChangeType == "9")
                {
                    return "變更(校正)週期";
                }
                else if (ChangeType == "A")
                {
                    return "變更(MSA)週期";
                }
                else if (ChangeType == "B")
                {
                    return "新增校驗";
                }
                else if (ChangeType == "C")
                {
                    return "新增MSA";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public FormInput FormInput { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            EquipmentList = new List<EquipmentModel>();
        }
    }
}
