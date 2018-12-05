using DbEntity.MSSQL.PipelinePatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class PipelineModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public string Color { get; set; }

        public List<PipelineLocus> Locus { get; set; }

        public List<PipelineSpecModel> SpecList { get; set; }

        public PipelineModel()
        {
            Locus = new List<PipelineLocus>();
            SpecList = new List<PipelineSpecModel>();
        }
    }
}
