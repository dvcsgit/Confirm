using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.OperationTable
{
   public class OperationTableGridItem
    {
       public int No { get; set; }


        /// <summary>
        /// 巡检结果序列码
        /// </summary>
        public string UniqueID { get; set; }
        /// <summary>
        /// 巡检日期
        /// </summary>
        public string CheckDate { get; set; }

        /// <summary>
        /// 巡检时间
        /// </summary>
        public string CheckTime { get; set; }

        /// <summary>
        /// 路线ID
        /// </summary>
        public string RouteID { get; set; }

        /// <summary>
        /// 路线名称
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// 路线
        /// </summary>
        public string Route
        {
            get
            {
                return RouteID + "/" + RouteName;
            }
        }
        /// <summary>
        /// 巡检点代号
        /// </summary>
        /// 

        public string ControlPointID { get; set; }
        /// <summary>
        /// 巡检点描述
        /// </summary>

        public string ControlPointDescription { get; set; }
        /// <summary>
        /// 巡检点
        /// </summary>

        public string ControlPoint
        {
            get
            {
                return ControlPointID + "/" + ControlPointDescription;

            }
        }
         /// <summary>
         /// 设备代号
         /// </summary>
         /// 

        public string EquipmentID { get; set; }
        /// <summary>
        /// 设备描述
        /// </summary>

        public string EquipmentName { get; set; }
        /// <summary>
        /// 设备
        /// </summary>

        public string Equipment
        {
            get
            {
                return EquipmentID + "/" + EquipmentName;

            }
        }


        /// <summary>
        /// 部位代号
        /// </summary>
        /// 

        public string PartUniqueID { get; set; }
        /// <summary>
        /// 部位描述
        /// </summary>

        public string PartDescription { get; set; }
        /// <summary>
        /// 部位
        /// </summary>

        public string Part
        {
            get
            {
                return PartUniqueID + "/" + PartDescription;

            }
        }

        /// <summary>
        /// 检查代号
        /// </summary>
        /// 

        public string CheckItemID { get; set; }
        /// <summary>
        /// 检查项目描述
        /// </summary>

        public string CheckItemDescription { get; set; }
        /// <summary>
        /// 检查项目
        /// </summary>

        public string CheckItem
        {
            get
            {
                return CheckItemID + "/" + CheckItemDescription;

            }
        }


        /// <summary>
        /// 检查结果
        /// </summary>
        /// 
       public string Result { get; set; }



        /// <summary>
        /// 异常代号
        /// </summary>
        /// 

        public string AbnormalReasonID { get; set; }
        /// <summary>
        /// 异常描述
        /// </summary>

        public string AbnormalReasonDescription { get; set; }

        /// <summary>
        /// 异常标记
        /// </summary>

        public string AbnormalReasonRemark { get; set; }

        /// <summary>
        /// 异常
        /// </summary>

        public string AbnormalReason
        {
            get
            {
                if (!string.IsNullOrEmpty(AbnormalReasonRemark))
                {
                    return string.Format("{0}/{1}", AbnormalReasonID, AbnormalReasonDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", AbnormalReasonID, AbnormalReasonRemark);
                }
            }
        }



        /// <summary>
        /// 处理对策代号
        /// </summary>
        /// 

        public string HandlingMethodID { get; set; }
        /// <summary>
        /// 处理对策描述
        /// </summary>

        public string HandlingMethodDescription { get; set; }
        /// <summary>
        /// 标记
        /// </summary>
        public string HandlingMethodRemark { get; set; }

        /// <summary>
        /// 处理对策
        /// </summary>

        public string HandlingMethod
        {
            get
            {
                if (!string.IsNullOrEmpty(HandlingMethodRemark))
                {
                    return string.Format("{0}/{1}", HandlingMethodID, HandlingMethodRemark);
                }
                else
                {
                    return string.Format("{0}/{1}", HandlingMethodID, HandlingMethodDescription);
                }
            }
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public string UserID { get; set; }
        public string UserName { get; set; }
        public string ArriveRecord
        {
            get
            {
                return string.Format("{0}/{1}", UserID, UserName);
            }
        }











        //public void Create()
        //{

        //}


    }
}
