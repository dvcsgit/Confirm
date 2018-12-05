using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public EquipmentModel Equipment { get; set; }

        public FormInput FormInput { get; set; }

        public Models.ASE.QA.CalibrationApply.GridItem CalibrationApply { get; set; }

        public List<Models.ASE.QA.CalibrationNotify.GridItem> CalibrationNotifyList { get; set; }

        public List<Models.ASE.QA.CalibrationForm.GridItem> CalibrationFormList { get; set; }

        public List<Models.ASE.QA.MSANotify.GridItem> MSANotifyList { get; set; }

        public List<Models.ASE.QA.MSAForm_v2.GridItem> MSAFormList { get; set; }

        public List<Models.ASE.QA.ChangeForm.GridItem> ChangeFormList { get; set; }

        public List<Models.ASE.QA.AbnormalForm.GridItem> AbnormalFormList { get; set; }

        public bool CanCAL
        {
            get
            {
                if (Equipment.CAL)
                {
                    if (CalibrationApply != null && CalibrationApply.Status.Status != "3" && CalibrationApply.Status.Status != "4")
                    {
                        return false;
                    }
                    else if (CalibrationFormList != null && CalibrationFormList.Any(x => x.Status.Status != "5" && x.Status.Status != "9"))
                    {
                        return false;
                    }
                    else if (CalibrationNotifyList != null && CalibrationNotifyList.Any(x => x.Status.Status != "3" && x.Status.Status != "4"))
                    {
                        return false;
                    }
                    else if (ChangeFormList != null && ChangeFormList.Any(x => x.Status != "2" && x.Status != "3"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanMSA
        {
            get
            {
                if (Equipment.MSA)
                {
                    if (CalibrationApply != null && CalibrationApply.Status.Status != "3" && CalibrationApply.Status.Status != "4")
                    {
                        return false;
                    }
                    else if (MSAFormList != null && MSAFormList.Any(x => x.Status.Status != "5" && x.Status.Status != "6"))
                    {
                        return false;
                    }
                    else if (MSANotifyList != null && MSANotifyList.Any(x => x.Status.Status != "3" && x.Status.Status != "4"))
                    {
                        return false;
                    }
                    else if (ChangeFormList != null && ChangeFormList.Any(x => x.Status != "2" && x.Status != "3"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public EditFormModel()
        {
            Equipment = new EquipmentModel();
            FormInput = new FormInput();
        }
    }
}
