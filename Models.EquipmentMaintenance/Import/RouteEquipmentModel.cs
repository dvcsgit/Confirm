using System.Collections.Generic;

namespace Models.EquipmentMaintenance.Import
{
    public class RouteEquipmentModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string PartUniqueID { get; set; }

        public string PartDescription { get; set; }

        public List<RouteEquipmentCheckItemModel> CheckItemList { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    if (string.IsNullOrEmpty(PartDescription))
                    {
                        return string.Format("{0}({1})", Name, ErrorMessage);
                    }
                    else
                    {
                        return string.Format("{0}-{1}({2})", Name,PartDescription, ErrorMessage);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(PartDescription))
                    {
                        return Name;
                    }
                    else
                    {
                        return string.Format("{0}-{1}", Name, PartDescription);
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
                return !IsExist || IsParentError;
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (!IsExist)
                {
                    return string.Format("{0} {1} {2}", Resources.Resource.EquipmentID, ID, Resources.Resource.NotExist);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public RouteEquipmentModel()
        {
            CheckItemList = new List<RouteEquipmentCheckItemModel>();
        }
    }
}
