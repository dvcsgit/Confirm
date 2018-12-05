using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Inspection
{
    public class DetailViewModel
    {
        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

         [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public string ConstructionFirmUniqueID { get; set; }

        public string ConstructionFirmName { get; set; }

        public string ConstructionFirmRemark { get; set; }

        [Display(Name = "ConstructionFirm", ResourceType = typeof(Resources.Resource))]
        public string ConstructionFirm
        {
            get
            {
                if (ConstructionFirmUniqueID == Define.OTHER)
                {
                    return ConstructionFirmRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(ConstructionFirmName))
                    {
                        return ConstructionFirmName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public string ConstructionTypeUniqueID { get; set; }

        public string ConstructionTypeDescription { get; set; }

        public string ConstructionTypeRemark { get; set; }

         [Display(Name = "ConstructionType", ResourceType = typeof(Resources.Resource))]
        public string ConstructionType
        {
            get
            {
                if (ConstructionTypeUniqueID == Define.OTHER)
                {
                    return ConstructionTypeRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(ConstructionTypeDescription))
                    {
                        return ConstructionTypeDescription;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

          [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDate { get; set; }

          [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDate { get; set; }

          [Display(Name = "Address", ResourceType = typeof(Resources.Resource))]
        public string Address { get; set; }

        public DateTime CreateTime { get; set; }

         [Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

         [Display(Name = "CreateUser", ResourceType = typeof(Resources.Resource))]
        public UserModel CreateUser { get; set; }

        public List<InspectionUserModel> InspectionUserList { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        public double? PipePointLAT { get; set; }

        public double? PipePointLNG { get; set; }

        public List<string> PhotoList { get; set; }

        public List<MessageModel> MessageList { get; set; }

        public DetailViewModel()
        {
            InspectionUserList = new List<InspectionUserModel>();
            PhotoList = new List<string>();
            MessageList = new List<MessageModel>();
        }
    }
}
