using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.ECOVE.Models.AbnormalHandlingManagement
{
    public class EditFormModel
    {
        public string CheckResultUniqueID { get; set; }
        public string FactoryID { get; set; }

        public string RouteID { get; set; }

        public string Description
        {
            get
            { 
                var tolerance= string.Empty;

                if (LowerLimit.HasValue ||LowerAlertLimit.HasValue||UpperAlertLimit.HasValue|| UpperLimit.HasValue)
                {
                    tolerance = "(";

                    if (LowerLimit.HasValue)
                    {
                        tolerance += "下限：" + LowerLimit.Value.ToString()+"，";
                    }

                    if (LowerAlertLimit.HasValue)
                    {
                        tolerance += "下限警戒值：" + LowerAlertLimit.Value.ToString() + "，";
                    }

                    if (UpperAlertLimit.HasValue)
                    {
                        tolerance += "上限警戒值：" + UpperAlertLimit.Value.ToString() + "，";
                    }

                    if (UpperLimit.HasValue)
                    {
                        tolerance += "上限：" + UpperLimit.Value.ToString() + "，";
                    }

                    tolerance.Remove(tolerance.Length - 1, 1);

                    tolerance += ")";
                }

                return string.Format("{0}，結果：{1}{2}{3}", CheckItemDescription, Result, Unit, tolerance);
            
            }
        }

        public string UniqueID { get; set; }

        public string Type
        {
            get
            {
                return "P";
            }
        }

        public string TypeDisplay
        {
            get
            {
                return "巡檢";
            }
        }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string ControlPointDisplay
        {
            get
            {
                return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
            }
        }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string EquipmentDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public string Display
        {
            get
            {
                if (string.IsNullOrEmpty(EquipmentID))
                {
                    return ControlPointDisplay;
                }
                else
                {
                    return EquipmentDisplay;
                }
            }
        }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItemDisplay
        {
            get
            {
                return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
            }
        }

        public string ItemDisplay
        {
            get
            {
                return CheckItemDisplay;
            }
        }

        public string Date { get; set; }

        public string Time { get; set; }

        public string CheckTimeDisplay
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(Date, Time));
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public string Result { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public UserModel CheckUser { get; set; }

        public bool IsClosed
        {
            get
            {
                return ClosedTime.HasValue;
            }
        }

        public DateTime? ClosedTime { get; set; }

        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public UserModel ClosedUser { get; set; }

        public string ClosedRemark { get; set; }

        public string VHNO { get; set; }

        public List<PhotoModel> BeforePhotoList { get; set; }

        public List<PhotoModel> AfterPhotoList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public string AbnormalReasons
        {
            get
            {
                if (AbnormalReasonList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var abnormalReason in AbnormalReasonList)
                    {
                        if (!string.IsNullOrEmpty(abnormalReason.HandlingMethods))
                        {
                            sb.Append(string.Format("{0}({1})", abnormalReason.Display, abnormalReason.HandlingMethods));
                        }
                        else
                        {
                            sb.Append(abnormalReason.Display);
                        }

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

        public List<UserModel> ChargePersonList { get; set; }

        public List<string> ChargePersonIDList
        {
            get
            {
                return ChargePersonList.Select(x => x.ID).ToList();
            }
        }

        public string ChargePersons
        {
            get
            {
                if (ChargePersonList != null && ChargePersonList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var chargePerson in ChargePersonList)
                    {
                        sb.Append(chargePerson.User);
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

        public EditFormModel()
        {
            BeforePhotoList = new List<PhotoModel>();
            AfterPhotoList = new List<PhotoModel>();
            FileList = new List<FileModel>();
            AbnormalReasonList = new List<AbnormalReasonModel>();
            ChargePersonList = new List<UserModel>();
        }
    }
}
