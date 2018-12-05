using DbEntity.MSSQL.PipelinePatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class DownloadDataModel
    {
        public string LastModifyTime { get; set; }

        public List<AbnormalReason> PipelineAbnormalReasonList { get; set; }

        public List<DialogModel> DialogList { get; set; }

        public List<MessageModel> MessageList { get; set; }

        public List<DbEntity.MSSQL.PipelinePatrol.Construction> ConstructionList { get; set; }

        public List<ConstructionFirm> ConstructionFirmList { get; set; }

        public List<ConstructionType> ConstructionTypeList { get; set; }

        public List<ConstructionPhotoModel> ConstructionPhotoList { get; set; }

        public List<DbEntity.MSSQL.PipelinePatrol.Inspection> InspectionList { get; set; }

        public List<InspectionPhotoModel> InspectionPhotoList { get; set; }

        public List<InspectionUser> InspectionUserList { get; set; }

        public List<DbEntity.MSSQL.PipelinePatrol.PipelineAbnormal> PipelineAbnormalList { get; set; }

        public List<PipelineAbnormalPhotoModel> PipelineAbnormalPhotoList { get; set; }

        public List<PipelineModel> PipelineList { get; set; }

        public List<PipePointModel> PipePointList { get; set; }

        public List<JobModel> JobList { get; set; }

        public List<RouteModel> RouteList { get; set; }

        /// <summary>
        /// 資料串接使用
        /// </summary>
        public List<UserModel> UserList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return RouteList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<AbnormalReasonModel> AbnormalReasonList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.AbnormalReasonList).Distinct().ToList();
            }
        }

        public List<HandlingMethodModel> HandlingMethodList
        {
            get
            {
                return AbnormalReasonList.SelectMany(x => x.HandlingMethodList).Distinct().ToList();
            }
        }

        public DownloadDataModel()
        {
            PipelineAbnormalReasonList = new List<AbnormalReason>();
            DialogList = new List<DialogModel>();
            MessageList = new List<MessageModel>();
            InspectionList = new List<DbEntity.MSSQL.PipelinePatrol.Inspection>();
            InspectionPhotoList = new List<InspectionPhotoModel>();
            InspectionUserList = new List<InspectionUser>();
            ConstructionList = new List<DbEntity.MSSQL.PipelinePatrol.Construction>();
            ConstructionPhotoList = new List<ConstructionPhotoModel>();
            ConstructionFirmList = new List<ConstructionFirm>();
            ConstructionTypeList = new List<ConstructionType>();
            PipelineAbnormalList = new List<DbEntity.MSSQL.PipelinePatrol.PipelineAbnormal>();
            PipelineAbnormalPhotoList = new List<PipelineAbnormalPhotoModel>();
            PipelineList = new List<PipelineModel>();
            PipePointList = new List<PipePointModel>();
            JobList = new List<JobModel>();
            RouteList = new List<RouteModel>();
            UserList = new List<UserModel>();
        }
    }
}
