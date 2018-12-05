using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.FPTC.Models.DispatchQuery
{
    public class GridItem
    {
        [DisplayName("異常")]
        public string Abnormal
        {
            get
            {
                if (HaveAbnormal)
                {
                    return "異常";
                }
                else if (HaveAlert)
                {
                    return "注意";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DisplayName("檢查完成率")]
        public string CompleteRate { get; set; }

        [DisplayName("公司")]
        public string Company
        {
            get
            {
                return string.Format("{0}/{1}", CompanyID, CompanyName);
            }
        }

        [DisplayName("廠處")]
        public string Department
        {
            get
            {
                return string.Format("{0}/{1}", DepartmentID, DepartmentName);
            }
        }

        [DisplayName("車牌")]
        public string CarNo { get; set; }

        [DisplayName("尾車")]
        public string SecondTruckNo { get; set; }

        [DisplayName("派車時間")]
        public string DispatchTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DispatchTime);
            }
        }

        [DisplayName("司機")]
        public string Driver { get; set; }

        [DisplayName("檢查人員")]
        public string CheckUser { get; set; }

        public bool IsChecked
        {
            get
            {
                return !string.IsNullOrEmpty(CheckUser);
            }
        }

       

        

        public string DispatchDate
        {
            get
            {
                return DateTimeHelper.DateTime2DateString(DispatchTime);
            }
        }

        public DateTime DispatchTime { get; set; }

        

        

        public string CompanyID { get; set; }

        public string CompanyName { get; set; }

        

        public string DepartmentID { get; set; }

        public string DepartmentName { get; set; }

        

        
        
        
        public string LabelClass { get; set; }
        public bool HaveAbnormal { get; set; }

        public bool HaveAlert { get; set; }
    }
}
