using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class RouteModel
    {
        public string OrganizationUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public List<RouteControlPointModel> ControlPointList { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return string.Format("{0}({1})", Name, ErrorMessage);
                }
                else
                {
                    return Name;
                }
            }
        }

        public bool IsExist { get; set; }

        public bool IsParentError { get; set; }

        public bool IsError
        {
            get
            {
                return 
                    IsExist || 
                    IsParentError || 
                    string.IsNullOrEmpty(ID) || 
                    (!string.IsNullOrEmpty(ID) && ID.Length > 32) || 
                    string.IsNullOrEmpty(Name) ||
                    (!string.IsNullOrEmpty(Name) && Name.Length > 64);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (IsExist)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.RouteID, Resources.Resource.Exists));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(ID))
                {
                    sb.Append(Resources.Resource.RouteIDRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.RouteID, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(Name))
                {
                    sb.Append(Resources.Resource.RouteNameRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Name) && Name.Length > 64)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.RouteName, Resources.Resource.Length, 64));
                    sb.Append("、");
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public RouteModel()
        {
            ControlPointList = new List<RouteControlPointModel>();
        }
    }
}
