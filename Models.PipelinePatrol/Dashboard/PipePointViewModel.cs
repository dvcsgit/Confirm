using Models.PipelinePatrol.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.Dashboard
{
    public class PipePointViewModel
    {
        public string UniqueID { get; set; }

        public string PointType { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public Location Location { get; set; }

        public PipePointViewModel()
        {
            Location = new Location();
        }
    }
}
