using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class ConstructionModel
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string InspectionUniqueID { get; set; }

        public string PipePointUniqueID { get; set; }

        public string ConstructionFirmUniqueID { get; set; }

        public string ConstructionFirmRemark { get; set; }

        public string ConstructionTypeUniqueID { get; set; }

        public string ConstructionTypeRemark { get; set; }

        public string Description { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public double LAT { get; set; }

        public double LNG { get; set; }

        public string Address { get; set; }

        public string CreateUserID { get; set; }

        public DateTime CreateTime { get; set; }

        public List<ConstructionPhotoModel> Photos { get; set; }

        public List<ConstructionFileModel> Files { get; set; }
    }
}
