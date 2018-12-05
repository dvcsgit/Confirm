using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class CheckPointModel
    {
        public string UniqueID { get; set; }

        public string PointType { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        public int? MinTimeSpan { get; set; }

        public string Remark { get; set; }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public CheckPointModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
