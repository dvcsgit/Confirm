using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QS.CheckListManagement
{
    public class CheckResultModel
    {
        public decimal Seq { get; set; }

        public string StationUniqueID { get; set; }

        public string Station { get; set; }

        public string AuditObject { get; set; }

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

        public string Result { get; set; }
        public string CarNo { get; set; }
        public string Remark { get; set; }

        public string CPNO { get; set; }

        public decimal? Weekly { get; set; }

        public string AuditType { get; set; }

        public string AuditTypeUniqueID { get; set; }

        public string AuditorShift { get; set; }

        public string AuditorShiftUniqueID { get; set; }

        public string BelongShift { get; set; }

        public string BelongShiftUniqueID { get; set; }

        public string CarOwnerID { get; set; }

        public string CarOwnerName { get; set; }

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

        public string Risk { get; set; }

        public string RiskUniqueID { get; set; }

        public string Grade { get; set; }

        public string GradeUniqueID { get; set; }

        public string IsBelongMO { get; set; }

        public string ErrorUserID { get; set; }

        public string ErrorUserName { get; set; }

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

        public string ErrorMachineNo { get; set; }

        public string ErrorArea { get; set; }

        public List<PhotoModel> PhotoList { get; set; }

        public CheckResultModel()
        {
            PhotoList = new List<PhotoModel>();
            ResDepartmentDescriptionList = new List<string>();
        }
    }
}
