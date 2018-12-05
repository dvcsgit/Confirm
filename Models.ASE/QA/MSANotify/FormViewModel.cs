using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSANotify
{
    /// <summary>
    /// Status
    /// 0：文審退回
    /// 1：簽核中
    /// 2：退回修正
    /// 3：簽核完成
    /// </summary>
    public class FormViewModel
    {
        [DisplayName("單號")]
        public string VHNO { get; set; }

        public NotifyStatus Status { get; set; }

        public EquipmentModel Equipment { get; set; }

        [DisplayName("廠別")]
        public string Factory { get; set; }

        [DisplayName("部門")]
        public string Department { get; set; }

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

        public string IchiUniqueID { get; set; }

        public string IchiName { get; set; }

        public string IchiRemark { get; set; }

        [DisplayName("儀器名稱")]
        public string Ichi
        {
            get
            {
                if (IchiUniqueID == Define.OTHER)
                {
                    return IchiRemark;
                }
                else
                {
                    return IchiName;
                }
            }
        }

        [DisplayName("量類")]
        public string CharacteristicType { get; set; }

        [DisplayName("序號")]
        public string SerialNo { get; set; }

        [DisplayName("機台編號")]
        public string MachineNo { get; set; }

        [DisplayName("廠牌")]
        public string Brand { get; set; }

        [DisplayName("型號")]
        public string Model { get; set; }

        [DisplayName("Spec")]
        public string Spec { get; set; }

        public DateTime? CreateTime { get; set; }

        [DisplayName("立案時間")]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

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

        [DisplayName("備註")]
        public string Remark { get; set; }

        [DisplayName("校正頻率(月)")]
        public int Cycle { get; set; }

        public string MSAResponsorID { get; set; }

        public string MSAResponsorName { get; set; }

        [DisplayName("MSA負責人員")]
        public string MSAResponsor
        {
            get
            {
                if (!string.IsNullOrEmpty(MSAResponsorID))
                {
                    if (!string.IsNullOrEmpty(MSAResponsorName))
                    {
                        return string.Format("{0}/{1}", MSAResponsorID, MSAResponsorName);
                    }
                    else
                    {
                        return MSAResponsorID;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DateTime? EstMSADate { get; set; }

        [DisplayName("預計MSA日期")]
        public string EstMSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstMSADate);
            }
        }

        public string MSAType { get; set; }

        public string MSASubType { get; set; }

        [DisplayName("類別")]
        public string MSATypeDisplay
        {
            get
            {
                if (MSAType == "1")
                {
                    if (MSASubType == "1")
                    {
                        return "計量(全距平均法)";
                    }
                    else if (MSASubType == "2")
                    {
                        return "計量(ANOVA)";
                    }
                    else
                    {
                        return "計量";
                    }
                }
                else if (MSAType == "2")
                {
                    return "計數";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string MSAStationUniqueID { get; set; }

        public string MSAStationName { get; set; }

        public string MSAStationRemark { get; set; }

        [DisplayName("站別")]
        public string MSAStationDisplay
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

        [DisplayName("儀器")]
        public string MSAIchiDisplay
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

        public List<MSACharacteristicModel> MSACharacteristicList { get; set; }

        [DisplayName("量測特性")]
        public string MSACharacteristicDisplay
        {
            get
            {
                if (MSACharacteristicList != null && MSACharacteristicList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var c in MSACharacteristicList)
                    {
                        sb.Append(c.Display);
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

        public List<LogModel> LogList { get; set; }

        public FormViewModel()
        {
            MSACharacteristicList = new List<MSACharacteristicModel>();
            LogList = new List<LogModel>();
        }
    }
}
