using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QS.CheckListManagement
{
    public class MonthlyReportItem
    {
        //[DisplayName("Repeated Case (Y/N)")]
        //public string IsRepeatedCase { get; set; }

        //[DisplayName("Repeated Times")]
        //public decimal? RepeatedTimes { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }

        [DisplayName("C.P No.")]
        public string CPNO { get; set; }

        [DisplayName("CAR No.")]
        public string CarNo { get; set; }

        [DisplayName("Weekly週別")]
        public decimal? Weekly { get; set; }

        [DisplayName("稽核性質")]
        public string AuditType { get; set; }

        public DateTime AuditDate { get; set; }

        [DisplayName("Audit Date日期")]
        public string AuditDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(AuditDate);
            }
        }

        public string ShiftUniqueID { get; set; }

        public string ShiftDescription { get; set; }

        public string ShiftRemark { get; set; }

        [DisplayName("Shift班別")]
        public string Shift
        {
            get
            {
                if (ShiftUniqueID == Define.OTHER)
                {
                    return ShiftRemark;
                }
                else
                {
                    return ShiftDescription;
                }
            }
        }

        public string FactoryUniqueID { get; set; }

        public string FactoryDescription { get; set; }

        public string FactoryRemark { get; set; }

        [DisplayName("Plant廠別")]
        public string Factory
        {
            get
            {
                if (FactoryUniqueID == Define.OTHER)
                {
                    return FactoryRemark;
                }
                else
                {
                    return FactoryDescription;
                }
            }
        }

        //[DisplayName("Section處別")]
        //public string Section
        //{
        //    get
        //    {
        //        return Factory;
        //    }
        //}

        public List<string> StationList { get; set; }

        [DisplayName("Station")]
        public string Stations
        {
            get
            {
                if (StationList != null && StationList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var station in StationList)
                    {
                        sb.Append(station);
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

        [DisplayName("Item稽核項目")]
        public decimal CheckTypeID { get; set; }

        [DisplayName("Detail Item稽核細項")]
        public decimal CheckItemID { get; set; }

        [DisplayName("Process製程站別")]
        public string AuditStation { get; set; }

        public string ResDepartments { get; set; }

        public List<string> ResDepartmentList
        {
            get
            {
                if (!string.IsNullOrEmpty(ResDepartments))
                {
                    return ResDepartments.Split(Define.Seperators, StringSplitOptions.None).ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public List<string> ResDepartmentDescriptionList { get; set; }

        [DisplayName("Dept.缺失部門(負責部門)")]
        public string ResDepartmentDescription
        {
            get
            {
                if (ResDepartmentDescriptionList != null && ResDepartmentDescriptionList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var item in ResDepartmentDescriptionList)
                    {
                        sb.Append(item);
                        sb.Append("/");
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

        [DisplayName("問題發生歸屬班別")]
        public string BelongShift { get; set; }

        public string CarOwnerID { get; set; }

        public string CarOwnerName { get; set; }

        [DisplayName("CAR Owner")]
        public string CarOwner
        {
            get
            {
                if (!string.IsNullOrEmpty(CarOwnerName))
                {
                    return string.Format("{0}/{1}", CarOwnerID, CarOwnerName);
                }
                else
                {
                    return CarOwnerID;
                }
            }
        }

        public string CarOwnerManagerID { get; set; }

        public string CarOwnerManagerName { get; set; }

        [DisplayName("CAR Owner' Boss")]
        public string CarOwnerManager
        {
            get
            {
                if (!string.IsNullOrEmpty(CarOwnerManagerName))
                {
                    return string.Format("{0}/{1}", CarOwnerManagerID, CarOwnerManagerName);
                }
                else
                {
                    return CarOwnerManagerID;
                }
            }
        }

        public string DepartmentManagerID { get; set; }

        public string DepartmentManagerName { get; set; }

        [DisplayName("Dept. Magr")]
        public string DepartmentManager
        {
            get
            {
                if (!string.IsNullOrEmpty(DepartmentManagerName))
                {
                    return string.Format("{0}/{1}", DepartmentManagerID, DepartmentManagerName);
                }
                else
                {
                    return DepartmentManagerID;
                }
            }
        }

        [DisplayName("Grade/Risk風險等級")]
        public string Risk { get; set; }

        [DisplayName("Grade")]
        public string Grade { get; set; }

        [DisplayName("Owner確認(Y/N)是否歸屬MO")]
        public string IsBelongMO { get; set; }

        //[DisplayName("(針對Major/MO)Owner 澄清")]
        //public string OwnerClarify { get; set; }

        [DisplayName("Describe稽核缺失內容")]
        public string Remark { get; set; }

        //[DisplayName("Temporary Corrective Action暫時性改善措施")]
        //public string S1 { get; set; }

        public string ErrorUserID { get; set; }

        public string ErrorUserName { get; set; }

        [DisplayName("Badge缺失人員工號")]
        public string ErrorUser
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorUserName))
                {
                    return string.Format("{0}/{1}", ErrorUserID, ErrorUserName);
                }
                else
                {
                    return ErrorUserID;
                }
            }
        }

        [DisplayName("M/C No.機台編號")]
        public string ErrorMachineNo { get; set; }

        [DisplayName("Location區域")]
        public string ErrorArea { get; set; }

        //[DisplayName("Recurrence Case再發項目")]
        //public string S5 { get; set; }

        //[DisplayName("Root Caues根本原因")]
        //public string S6 { get; set; }

        //[DisplayName("Corrective Action改善措施")]
        //public string S7 { get; set; }

        //[DisplayName("Action Owner")]
        //public string S8 { get; set; }

        //[DisplayName("Target Date到期日")]
        //public string S9 { get; set; }

        //[DisplayName("已回覆")]
        //public string S10 { get; set; }

        //[DisplayName("CAR Reply OTD系統回覆準時性")]
        //public string S11 { get; set; }

        //[DisplayName("CAR Validated Date覆檢日")]
        //public string S12 { get; set; }

        //[DisplayName("CAR Validated Result覆檢結果")]
        //public string S13 { get; set; }

        //[DisplayName("Description of Validation Observation改善情況")]
        //public string S14 { get; set; }

        //[DisplayName("Picture缺失照片")]
        //public string S15 { get; set; }

        //[DisplayName("Category種類")]
        //public string S16 { get; set; }

        //[DisplayName("Month月")]
        //public string S17 { get; set; }

        //[DisplayName("Quarter季")]
        //public string S18 { get; set; }

        //[DisplayName("Year年")]
        //public string S19 { get; set; }

        public List<PhotoModel> PhotoList { get; set; }

        public MonthlyReportItem()
        {
            PhotoList = new List<PhotoModel>();
        }
    }
}
