using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class ControlPointModel
    {
        public string OrganizationUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public string TagID { get; set; }

        public string Remark { get; set; }

        public List<ControlPointCheckItemModel> CheckItemList { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return string.Format("{0}({1})", Description, ErrorMessage);
                }
                else
                {
                    return Description;
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
                    string.IsNullOrEmpty(Description) ||
                    (!string.IsNullOrEmpty(Description) && Description.Length > 64);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (IsExist)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.ControlPointID, Resources.Resource.Exists));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(ID))
                {
                    sb.Append(Resources.Resource.ControlPointIDRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.ControlPointID, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(Description))
                {
                    sb.Append(Resources.Resource.ControlPointDescriptionRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Description) && Description.Length > 64)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.ControlPointDescription, Resources.Resource.Length, 64));
                    sb.Append("、");
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public ControlPointModel()
        {
            CheckItemList = new List<ControlPointCheckItemModel>();
        }
    }
}
