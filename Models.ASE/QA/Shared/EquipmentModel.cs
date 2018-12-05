using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.Shared
{
    public class EquipmentModel
    {
        public string UniqueID { get; set; }

        public string Quatation
        {
            get
            {
                var fileName = string.Format("{0}.pdf", UniqueID);

                var filePath = Path.Combine(Config.QAFileFolderPath, fileName);

                return filePath;
            }
        }

        public bool IsQuatationExist
        {
            get
            {
                return File.Exists(Quatation);
            }
        }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "1")
                {
                    if (IsDelay)
                    {
                        return "2";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string StatusDescription
        {
            get
            {
                switch (StatusCode)
                { 
                    case "1":
                        return "正常";
                    case "2":
                        return "逾期";
                    case "3":
                        return "免校驗";
                    case "4":
                        return "遺失";
                    case "5":
                        return "報廢";
                    case "6":
                        return "維修中";
                    case "7":
                        return "庫存";
                    default:
                        return "-";
                }
            }
        }

        //校驗編號
        public string CalNo { get; set; }

        public string MSACalNo { get; set; }

        public string Factory { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string OrganizationFullDescription { get; set; }

        public string FactoryID { get; set; }

        public string IchiType { get; set; }

        [Display(Name = "IchiType", ResourceType = typeof(Resources.Resource))]
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

        public string CharacteristicType { get; set; }

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

        [Display(Name = "IchiName", ResourceType = typeof(Resources.Resource))]
        public string IchiDisplay
        {
            get
            {
                if (IchiUniqueID == Define.OTHER)
                {
                    return string.Format("({0}){1}", Resources.Resource.Other, IchiRemark);
                }
                else
                {
                    return IchiName;
                }
            }
        }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [DisplayName("機台編號")]
        public string MachineNo { get; set; }

        [DisplayName("Spec")]
        public string Spec { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }

        //public int Cycle { get; set; }

        public int? CalCycle { get; set; }

        public int? MSACycle { get; set; }

        public string Remark { get; set; }

        [Display(Name = "Calibration", ResourceType = typeof(Resources.Resource))]
        public bool CAL { get; set; }

        [Display(Name = "MSA", ResourceType = typeof(Resources.Resource))]
        public bool MSA { get; set; }

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

        public DateTime? LastCalDate { get; set; }

        public string LastCalDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(LastCalDate);
            }
        }

        public DateTime? NextCalDate { get; set; }

        public string NextCalDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NextCalDate);
            }
        }

        public DateTime? LastMSADate { get; set; }

        public string LastMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(LastMSADate);
            }
        }

        public DateTime? NextMSADate { get; set; }

        public string NextMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NextMSADate);
            }
        }

        public string Extension { get; set; }

        public string PhotoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(Extension))
                {
                    return Path.Combine(Config.QAFileFolderPath, PhotoName);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string PhotoName
        {
            get
            {
                if (!string.IsNullOrEmpty(Extension))
                {
                    return string.Format("{0}.{1}", UniqueID, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsDelay { get; set; }

        public string Display
        {
            get
            {
                return string.Format("[{0}][{1}]{2}", StatusDescription, CalNo, IchiDisplay);
            }
        }

        public string MSAType { get; set; }

        public string MSASubType { get; set; }

        public string MSAStationUniqueID { get; set; }

        public string MSAStationName { get; set; }
        
        public string MSAStationRemark { get; set; }

        public string MSAStation
        {
            get
            {
                if (MSAStationUniqueID == Define.OTHER)
                {
                    return MSAStationRemark;
                }
                else
                {
                    return MSAStationName;
                }
            }
        }

        public string MSAIchiUniqueID { get; set; }

        public string MSAIchiName { get; set; }

        public string MSAIchiRemark { get; set; }

        public string MSAIchi
        {
            get
            {
                if (MSAIchiUniqueID == Define.OTHER)
                {
                    return MSAIchiRemark;
                }
                else
                {
                    return MSAIchiName;
                }
            }
        }
    }
}
