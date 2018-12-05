using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.PipelineAbnormal
{
    public class DetailViewModel
    {
        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public string AbnormalReasonUniqueID { get; set; }

        public string AbnormalReasonDescription { get; set; }

        public string AbnormalReasonRemark { get; set; }

        [Display(Name = "AbnormalReason", ResourceType = typeof(Resources.Resource))]
        public string AbnormalReason
        {
            get
            {
                if (AbnormalReasonUniqueID == Define.OTHER)
                {
                    return AbnormalReasonRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(AbnormalReasonDescription))
                    {
                        return AbnormalReasonDescription;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

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

        public double LNG { get; set; }

        public double LAT { get; set; }

        public double? PipePointLAT { get; set; }

        public double? PipePointLNG { get; set; }

        public List<string> PhotoList { get; set; }

        public List<MessageModel> MessageList { get; set; }

        public DetailViewModel()
        {
            PhotoList = new List<string>();
            MessageList = new List<MessageModel>();
        }
    }
}
