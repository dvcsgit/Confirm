using Models.PipelinePatrol.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.Linq;

namespace Models.PipelinePatrol.PipelineManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "PipelineID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        public Location Max
        {
            get
            {
                if (Locus.Count > 0)
                {
                    return new Location()
                    {
                        LAT = Locus.Max(x => x.LAT),
                        LNG = Locus.Max(x => x.LNG)
                    };
                }
                else
                {
                    return new Location() { LAT = 0, LNG = 0 };
                }
            }
        }

        public Location Min
        {
            get
            {
                if (Locus.Count > 0)
                {
                    return new Location()
                    {
                        LAT = Locus.Min(x => x.LAT),
                        LNG = Locus.Min(x => x.LNG)
                    };
                }
                else
                {
                    return new Location() { LAT = 0, LNG = 0 };
                }
            }
        }

        public List<Location> Locus { get; set; }

        public string Color { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            Locus = new List<Location>();
        }
    }
}
