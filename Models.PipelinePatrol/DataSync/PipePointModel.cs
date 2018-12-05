using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class PipePointModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string PointType { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        public string Remark { get; set; }

        public List<PipePointFileModel> FileList { get; set; }

        public PipePointModel()
        {
            FileList = new List<PipePointFileModel>();
        }
    }
}
