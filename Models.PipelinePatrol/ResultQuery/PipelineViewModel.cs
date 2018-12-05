using Models.PipelinePatrol.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.ResultQuery
{
    public class PipelineViewModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Color { get; set; }

        public List<Location> Locus { get; set; }

        public PipelineViewModel()
        {
            Locus = new List<Location>();
        }
    }
}
