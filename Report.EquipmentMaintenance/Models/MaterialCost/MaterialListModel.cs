using System.ComponentModel;
using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.MaterialCost
{
    //public class MaterialReportModel
    //{
        public class MaterialListModel
        {
            /// <summary>
            /// 修復單編號
            /// </summary>
            public string RformVHNO { get; set; }

            /// <summary>
            /// 修復單類型
            /// </summary>
            public string RFormTypeName { get; set; }

            /// <summary>
            /// 設備ID
            /// </summary>
            public string EquipmentUniqueID { get; set; }

            /// <summary>
            /// 設備名稱
            /// </summary>
            public string EquipmentName { get; set; }

            /// <summary>
            /// 部位編號
            /// </summary>
            public string PartUniqueID { get; set; }

            /// <summary>
            /// 部位名稱
            /// </summary>
            public string PartName { get; set; }

            /// <summary>
            /// 本單建立時間
            /// </summary>
            public string CreateDate { get; set; }

            /// <summary>
            /// 材料編號
            /// </summary>
            public string MaterialID { get; set; }

            /// <summary>
            /// 材料品名
            /// </summary>
            public string MaterialName { get; set; }

            /// <summary>
            /// 材料規格
            /// </summary>
            public string MaterialValue { get; set; }

            /// <summary>
            /// 材料數量
            /// </summary>
            public int? MaterialQTY { get; set; }
        //}

        //public class MaterialReportModel
        //{
        //    /// <summary>
        //    /// 材料編號
        //    /// </summary>
        //    public string MaterialUniqueID { get; set; }

        //    /// <summary>
        //    /// 材料品名
        //    /// </summary>
        //    public string MaterialName { get; set; }

        //    /// <summary>
        //    /// 材料規格
        //    /// </summary>
        //    public string MaterialSpec { get; set; }

        //    /// <summary>
        //    /// 單價
        //    /// </summary>
        //    public string Price { get; set; }

        //    /// <summary>
        //    /// 總價
        //    /// </summary>
        //    public string TotalPrice { get; set; }

        //    ///// <summary>
        //    ///// 更換數量
        //    ///// </summary>
        //    //public string Total { get; set; }
        //}

        //public class MaterialViewModel
        //{
        //    public List<MaterialListModel> MaterialList { get; set; }
        //    public List<MaterialReportModel> MaterialReport { get; set; }
        //}
    }
}
