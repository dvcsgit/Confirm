using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class EquipmentModel
    {
        public string OrganizationUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string PartUniqueID { get; set; }

        public string PartDescription { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public List<EquipmentCheckItemModel> CheckItemList { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    if (!string.IsNullOrEmpty(PartDescription) && PartDescription != "*")
                    {
                        return string.Format("{0}-{1}({2})", Name, PartDescription, ErrorMessage);
                    }
                    else
                    {
                        return string.Format("{0}({1})", Name, ErrorMessage);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(PartDescription) && PartDescription != "*")
                    {
                        return string.Format("{0}-{1}", Name, PartDescription);
                    }
                    else
                    {
                        return string.Format("{0}", Name);
                    }
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
                    (!string.IsNullOrEmpty(Name) && Name.Length > 64) ||
                    (!string.IsNullOrEmpty(PartDescription) && PartDescription.Length > 64);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (IsExist)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.EquipmentID, Resources.Resource.Exists));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(ID))
                {
                    sb.Append(Resources.Resource.EquipmentIDRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(ID) && ID.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.EquipmentID, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (string.IsNullOrEmpty(Name))
                {
                    sb.Append(Resources.Resource.EquipmentNameRequired);
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Name) && Name.Length > 64)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.EquipmentName, Resources.Resource.Length, 64));
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(PartDescription) && PartDescription.Length > 64)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.PartDescription, Resources.Resource.Length, 64));
                    sb.Append("、");
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public EquipmentModel()
        {
            CheckItemList = new List<EquipmentCheckItemModel>();
        }
    }
}
