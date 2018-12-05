using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Utility;
namespace Models.TankPatrol.CheckItemManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "CheckType", ResourceType = typeof(Resources.Resource))]
        public string CheckType { get; set; }

        [Display(Name = "CheckItemID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "CheckItemDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "IsFeelItem", ResourceType = typeof(Resources.Resource))]
        public bool IsFeelItem { get; set; }

        public string IsFeelItemDisplay
        {
            get
            {
                if (IsFeelItem)
                {
                    return "選項";
                }
                else
                {
                    return "輸入值";
                }
            }
        }

        [Display(Name = "UpperLimit", ResourceType = typeof(Resources.Resource))]
        public string UpperLimit { get; set; }

        [Display(Name = "UpperAlertLimit", ResourceType = typeof(Resources.Resource))]
        public string UpperAlertLimit { get; set; }

        [Display(Name = "LowerAlertLimit", ResourceType = typeof(Resources.Resource))]
        public string LowerAlertLimit { get; set; }

        [Display(Name = "LowerLimit", ResourceType = typeof(Resources.Resource))]
        public string LowerLimit { get; set; }

        [Display(Name = "IsAccumulation", ResourceType = typeof(Resources.Resource))]
        public bool IsAccumulation { get; set; }

        public int? TextValueType { get; set; }

        public string TextValueTypeDisplay
        {
            get
            {
                if (TextValueType.HasValue)
                {
                    if (TextValueType.Value == 1)
                    {
                        return "數值";
                    }
                    else if (TextValueType.Value == 2)
                    {
                        return "文字";
                    }
                    else if (TextValueType.Value == 3)
                    {
                        return "日期";
                    }
                    else if (TextValueType.Value == 4)
                    {
                        return "時間";
                    }
                    else if (TextValueType.Value == 5)
                    {
                        return "日期+時間";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [Display(Name = "AccumulationBase", ResourceType = typeof(Resources.Resource))]
        public string AccumulationBase { get; set; }

        [Display(Name = "Unit", ResourceType = typeof(Resources.Resource))]
        public string Unit { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public List<string> AbnormalReasonDescriptionList { get; set; }

        [Display(Name = "AbnormalReason", ResourceType = typeof(Resources.Resource))]
        public string AbnormalReasons
        {
            get
            {
                if (AbnormalReasonDescriptionList != null && AbnormalReasonDescriptionList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var abnormalReason in AbnormalReasonDescriptionList)
                    {
                        sb.Append(abnormalReason);

                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<string> FeelOptionDescriptionList { get; set; }

        [Display(Name = "FeelOptions", ResourceType = typeof(Resources.Resource))]
        public string FeelOptions
        {
            get
            {
                if (FeelOptionDescriptionList != null && FeelOptionDescriptionList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var feelOption in FeelOptionDescriptionList)
                    {
                        sb.Append(feelOption);

                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            AbnormalReasonDescriptionList = new List<string>();
            FeelOptionDescriptionList = new List<string>();
        }
    }
}
