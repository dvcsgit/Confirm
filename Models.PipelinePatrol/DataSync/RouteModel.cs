using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class RouteModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Remark { get; set; }

        public List<CheckPointModel> CheckPointList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return CheckPointList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<string> PipelineList { get; set; }

        public RouteModel()
        {
            CheckPointList = new List<CheckPointModel>();
            PipelineList = new List<string>();
        }
    }
}
