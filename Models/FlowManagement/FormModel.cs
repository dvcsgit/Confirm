using Utility;

namespace Models.FlowManagement
{
    public class FormModel
    {
        public Define.EnumForm Form { get; set; }

        public string RepairFormTypeUniqueID { get; set; }

        public string RepairFormTypeDescription { get; set; }

        public string FormDescription { get; set; }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(RepairFormTypeDescription))
                {
                    return string.Format("{0}({1})", FormDescription, RepairFormTypeDescription);
                }
                else
                {
                    return FormDescription;
                }
            }
        }
    }
}
