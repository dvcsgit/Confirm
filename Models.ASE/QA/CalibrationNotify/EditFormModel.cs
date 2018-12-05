using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }


        public string FormUniqueID { get; set; }
        public string FormVHNO { get; set; }

        public EquipmentModel Equipment { get; set; }

        public string Status { get; set; }

        [DisplayName("狀態")]
        public string StatusDisplay
        {
            get
            {
                if (Status == "0")
                {
                    return "退回修正";
                }
                else if (Status == "1")
                {
                    return "簽核中";
                }
                else if (Status == "2")
                {
                    return "退回修正";
                }
                else if (Status == "3")
                {
                    return "簽核完成";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DisplayName("廠別")]
        public string Factory { get; set; }

        [DisplayName("部門")]
        public string Department { get; set; }

        [DisplayName("校驗編號")]
        public string CalNo { get; set; }

        public DateTime? CreateTime { get; set; }

        [DisplayName("立案時間")]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public List<LogModel> LogList { get; set; }

        #region FormInput
        public string FactoryID { get; set; }

        public string IchiType { get; set; }

        [DisplayName("儀器類別")]
        public string IchiTypeDisplay
        {
            get
            {
                if (IchiType == Define.OTHER)
                {
                    return Resources.Resource.Other;
                }
                else
                {
                    return string.Format("{0}{1}", FactoryID, IchiType);
                }
            }
        }

        [DisplayName("機台編號")]
        public string MachineNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }

        [DisplayName("Spec")]
        public string Spec { get; set; }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

        [DisplayName("設備負責人")]
        public string Owner
        {
            get
            {
                if (!string.IsNullOrEmpty(OwnerName))
                {
                    return string.Format("{0}/{1}", OwnerID, OwnerName);
                }
                else
                {
                    return OwnerID;
                }
            }
        }

        public string OwnerManagerID { get; set; }

        public string OwnerManagerName { get; set; }

        [DisplayName("設備負責人主管")]
        public string OwnerManager
        {
            get
            {
                if (!string.IsNullOrEmpty(OwnerManagerName))
                {
                    return string.Format("{0}/{1}", OwnerManagerID, OwnerManagerName);
                }
                else
                {
                    return OwnerManagerID;
                }
            }
        }

        public string PEID { get; set; }

        public string PEName { get; set; }

        [DisplayName("製程負責人")]
        public string PE
        {
            get
            {
                if (!string.IsNullOrEmpty(PEName))
                {
                    return string.Format("{0}/{1}", PEID, PEName);
                }
                else
                {
                    return PEID;
                }
            }
        }

        public string PEManagerID { get; set; }

        public string PEManagerName { get; set; }

        [DisplayName("製程負責人主管")]
        public string PEManager
        {
            get
            {
                if (!string.IsNullOrEmpty(PEManagerName))
                {
                    return string.Format("{0}/{1}", PEManagerID, PEManagerName);
                }
                else
                {
                    return PEManagerID;
                }
            }
        }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        [Display(Name = "CalibrationCycle", ResourceType = typeof(Resources.Resource))]
        public int Cycle { get; set; }
        #endregion

        public List<DetailItem> ItemList { get; set; }

        public List<SelectListItem> IchiSelectItemList { get; set; }
        public List<SelectListItem> CharacteristicTypeSelectItemList { get; set; }
        public List<CharacteristicTypeModel> CharacteristicTypeList { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            LogList = new List<LogModel>();
            ItemList = new List<DetailItem>();
            IchiSelectItemList = new List<SelectListItem>();
            CharacteristicTypeSelectItemList = new List<SelectListItem>();
            CharacteristicTypeList = new List<CharacteristicTypeModel>();
            FormInput = new FormInput();
        }
    }
}
