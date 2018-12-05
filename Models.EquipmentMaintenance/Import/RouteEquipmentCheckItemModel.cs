namespace Models.EquipmentMaintenance.Import
{
    public class RouteEquipmentCheckItemModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

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
                return !IsExist||IsParentError;
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (!IsExist)
                {
                    return string.Format("{0} {1} {2}", Resources.Resource.CheckItemID, ID, Resources.Resource.NotExist);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
