using Models.PipelinePatrol.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.Dashboard
{
    public class DashboardViewModel
    {
        public int? DefaultZoom { get; set; }

        public Location DefaultMapCenter { get; set; }

        public List<PipelineViewModel> PipelineList { get; set; }

        public List<PipePointViewModel> PipePointList { get; set; }

        public List<string> PipePointTypeList
        {
            get
            {
                return PipePointList.Select(x => x.PointType).Distinct().OrderBy(x => x).ToList();
            }
        }

        public List<PipePointViewModel> GetPipePointList(string PipePointType)
        {
            return PipePointList.Where(x => x.PointType == PipePointType).ToList();
        }

        public List<PipelineAbnormalViewModel> PipelineAbnormalList { get; set; }

        public List<ConstructionViewModel> ConstructionList { get; set; }

        public List<InspectionViewModel> InspectionList { get; set; }

        public List<InspectionViewModel> InspectedList
        {
            get
            {
                return InspectionList.Where(x => x.IsInspected).ToList();
            }
        }

        public List<InspectionViewModel> InspectYetList
        {
            get
            {
                return InspectionList.Where(x => !x.IsInspected).ToList();
            }
        }

        public List<OnlineUserViewModel> OnlineUserList { get; set; }

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

        public List<Location> Locus
        {
            get
            {
                return PipelineList.SelectMany(x => x.Locus).ToList();
            }
        }

        public DashboardViewModel()
        {
            PipelineList = new List<PipelineViewModel>();
            PipePointList = new List<PipePointViewModel>();
            PipelineAbnormalList = new List<PipelineAbnormalViewModel>();
            ConstructionList = new List<ConstructionViewModel>();
            InspectionList = new List<InspectionViewModel>();
            OnlineUserList = new List<OnlineUserViewModel>();
        }
    }
}
