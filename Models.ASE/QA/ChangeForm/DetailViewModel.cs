using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.ChangeForm
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Status { get; set; }

         [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string StatusDescription
        {
            get
            {
                if (Status == "1")
                {
                    return Resources.Resource.ChangeFormStatus_1;
                }
                else if (Status == "2")
                {
                    return "異動退回";
                }
                else if (Status == "3")
                {
                    return Resources.Resource.ChangeFormStatus_3;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ChangeType { get; set; }

        [Display(Name = "ChangeType", ResourceType = typeof(Resources.Resource))]
        public string ChangeTypeDescription
        {
            get
            {
                if (ChangeType == "1")
                {
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

        [DisplayName("廠別")]
        public string Factory { get; set; }

        [Display(Name = "Department", ResourceType = typeof(Resources.Resource))]
        public string OrganizationDescription { get; set; }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

        [Display(Name = "EquipmentOwner", ResourceType = typeof(Resources.Resource))]
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

        [Display(Name = "EquipmentOwnerManager", ResourceType = typeof(Resources.Resource))]
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

        [Display(Name = "PE", ResourceType = typeof(Resources.Resource))]
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

        [Display(Name = "PEManager", ResourceType = typeof(Resources.Resource))]
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

        public DateTime? FixFinishedDate { get; set; }

        [Display(Name = "FixFinishedDate", ResourceType = typeof(Resources.Resource))]
        public string FixFinishedDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(FixFinishedDate);
            }
        }

        public DateTime CreateTime { get; set; }

        [Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        [Display(Name = "ChangeReason", ResourceType = typeof(Resources.Resource))]
        public string ChangeReason { get; set; }

        public List<EquipmentModel> ItemList { get; set; }

        public List<LogModel> LogList { get; set; }

        public string NewOwnerID { get; set; }

        public string NewOwnerName { get; set; }

        public string NewOwner
        {
            get
            {
                if (!string.IsNullOrEmpty(NewOwnerName))
                {
                    return string.Format("{0}/{1}", NewOwnerID, NewOwnerName);
                }
                else
                {
                    return NewOwnerID;
                }
            }
        }

        public string NewOwnerManagerID { get; set; }

        public string NewOwnerManagerName { get; set; }

        public string NewOwnerManager
        {
            get
            {
                if (!string.IsNullOrEmpty(NewOwnerManagerName))
                {
                    return string.Format("{0}/{1}", NewOwnerManagerID, NewOwnerManagerName);
                }
                else
                {
                    return NewOwnerManagerID;
                }
            }
        }

        public string NewPEID { get; set; }

        public string NewPEName { get; set; }

        public string NewPE
        {
            get
            {
                if (!string.IsNullOrEmpty(NewPEName))
                {
                    return string.Format("{0}/{1}", NewPEID, NewPEName);
                }
                else
                {
                    return NewPEID;
                }
            }
        }

        public string NewPEManagerID { get; set; }

        public string NewPEManagerName { get; set; }

        public string NewPEManager
        {
            get
            {
                if (!string.IsNullOrEmpty(NewPEManagerName))
                {
                    return string.Format("{0}/{1}", NewPEManagerID, NewPEManagerName);
                }
                else
                {
                    return NewPEManagerID;
                }
            }
        }

        public int? NewCALCycle { get; set; }

        public int? NewMSACycle { get; set; }

        public DetailViewModel()
        {
            ItemList = new List<EquipmentModel>();
            LogList = new List<LogModel>();
        }
    }
}