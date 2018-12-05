using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
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

        public string FormUniqueID { get; set; }
        public string FormVHNO { get; set; }

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
                    return string.Format("({0}){1}", Resources.Resource.Other, IchiRemark);
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

        public string CalibrateType { get; set; }

        [DisplayName("類別")]
        public string CalibrateTypeDisplay
        {
            get
            {
                if (CalibrateType == "IF")
                {
                    //return Resources.Resource.CalibrateType_IF;
                    return "內校(現場)";
                }
                else if (CalibrateType == "IL")
                {
                    //return Resources.Resource.CalibrateType_IL;
                    return "內校(實驗室)";
                }
                else if (CalibrateType == "EF")
                {
                    //return Resources.Resource.CalibrateType_EF;
                    return "外校(現場)";
                }
                else if (CalibrateType == "EL")
                {
                    //return Resources.Resource.CalibrateType_EL;
                    return "外校(實驗室)";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string CalibrateUnit { get; set; }

        [DisplayName("校驗負責單位")]
        public string CalibrateUnitDisplay
        {
            get
            {
                if (CalibrateUnit == "F")
                {
                    //return Resources.Resource.CalibrateUnit_F;
                    return "現場";
                }
                else if (CalibrateUnit == "L")
                {
                    //return Resources.Resource.CalibrateUnit_L;
                    return "實驗室";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string CaseType { get; set; }

        [DisplayName("案件類別")]
        public string CaseTypeDisplay
        {
            get
            {
                if (CaseType == "G")
                {
                    //return Resources.Resource.CaseType_G;
                    return "一般";
                }
                else if (CaseType == "E")
                {
                    //return Resources.Resource.CaseType_E;
                    return "緊急";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

        [DisplayName("校驗負責人員")]
        public string Calibrator
        {
            get
            {
                if (!string.IsNullOrEmpty(CalibratorID))
                {
                    if (!string.IsNullOrEmpty(CalibratorName))
                    {
                        return string.Format("{0}/{1}", CalibratorID, CalibratorName);
                    }
                    else
                    {
                        return CalibratorID;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DateTime? EstCalibrateDate { get; set; }

        [DisplayName("預計校驗日期")]
        public string EstCalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstCalibrateDate);
            }
        }

        public List<DetailItem> ItemList { get; set; }

        public List<LogModel> LogList { get; set; }

        public FormViewModel()
        {
            ItemList = new List<DetailItem>();
            LogList = new List<LogModel>();
        }
    }
}
