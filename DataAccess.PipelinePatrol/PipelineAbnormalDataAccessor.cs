using DbEntity.MSSQL.PipelinePatrol;
using Models.PipelinePatrol.PipelineAbnormal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.PipelinePatrol
{
    public class PipelineAbnormalDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var query = (from a in db.PipelineAbnormal
                                 join r in db.AbnormalReason
                                 on a.AbnormalReasonUniqueID equals r.UniqueID into tmpAbnormalReason
                                 from r in tmpAbnormalReason.DefaultIfEmpty()
                                 select new
                                 {
                                     a.UniqueID,
                                     a.VHNO,
                                     a.Description,
                                     a.Address,
                                     a.AbnormalReasonUniqueID,
                                     AbnormalReasonDescription = r != null ? r.Description : string.Empty,
                                     a.AbnormalReasonRemark,
                                     a.CreateUserID,
                                     a.CreateTime
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    var queryResult = query.OrderByDescending(x => x.CreateTime).ToList();

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = queryResult.Select(x => new GridItem
                        {
                            UniqueID = x.UniqueID,
                            VHNO = x.VHNO,
                            Description = x.Description,
                            Address = x.Address,
                             AbnormalReasonUniqueID=x.AbnormalReasonUniqueID,
                              AbnormalReasonDescription = x.AbnormalReasonDescription,
                               AbnormalReasonRemark =x.AbnormalReasonRemark,
                            CreateTime = x.CreateTime,
                            CreateUser = UserDataAccessor.GetUser(x.CreateUserID)
                        }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var query = (from a in db.PipelineAbnormal
                                 join p in db.PipePoint
                                 on a.PipePointUniqueID equals p.UniqueID into tmpPipePoint
                                 from p in tmpPipePoint.DefaultIfEmpty()
                                 join r in db.AbnormalReason
                                 on a.AbnormalReasonUniqueID equals r.UniqueID into tmpAbnormalReason
                                 from r in tmpAbnormalReason.DefaultIfEmpty()
                                 where a.UniqueID == UniqueID
                                 select new
                                 {
                                     a.UniqueID,
                                     a.VHNO,
                                     a.Description,
                                     a.Address,
                                     a.LNG,
                                     a.LAT,
                                     PipePointLAT = p != null ? p.LAT : default(double?),
                                     PipePointLNG = p != null ? p.LNG : default(double?),
                                     a.AbnormalReasonUniqueID,
                                     AbnormalReasonDescription = r != null ? r.Description : string.Empty,
                                     a.AbnormalReasonRemark,
                                     a.CreateUserID,
                                     a.CreateTime
                                 }).First();

                    var model = new DetailViewModel()
                    {
                        VHNO = query.VHNO,
                        Description = query.Description,
                        Address = query.Address,
                        LNG = query.LNG,
                        LAT = query.LAT,
                        PipePointLAT = query.PipePointLAT,
                        PipePointLNG = query.PipePointLNG,
                        AbnormalReasonUniqueID = query.AbnormalReasonUniqueID,
                        AbnormalReasonDescription = query.AbnormalReasonDescription,
                        AbnormalReasonRemark = query.AbnormalReasonRemark,
                        CreateTime = query.CreateTime,
                        CreateUser = UserDataAccessor.GetUser(query.CreateUserID)
                    };

                    var photoList = db.PipelineAbnormalPhoto.Where(x => x.PipelineAbnormalUniqueID == UniqueID).OrderBy(x => x.Seq).ToList();

                    foreach (var photo in photoList)
                    {
                        model.PhotoList.Add(string.Format("{0}_{1}.{2}", photo.PipelineAbnormalUniqueID, photo.Seq, photo.Extension));
                    }

                    var dialog = db.Dialog.FirstOrDefault(x => x.PipelineAbnormalUniqueID == UniqueID);

                    if (dialog != null)
                    {
                        var messageList = db.Message.Where(x => x.DialogUniqueID == dialog.UniqueID).OrderBy(x => x.Seq).ToList();

                        foreach (var message in messageList)
                        {
                            model.MessageList.Add(new MessageModel()
                            {
                                User = UserDataAccessor.GetUser(message.UserID),
                                Message = message.Message1,
                                MessageTime = message.MessageTime,
                                DialogUniqueID = message.DialogUniqueID,
                                Seq = message.Seq,
                                Extension = message.Extension
                            });
                        }
                    }

                    result.ReturnData(model);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
