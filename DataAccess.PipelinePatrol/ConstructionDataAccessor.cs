using DbEntity.MSSQL.PipelinePatrol;
using Models.PipelinePatrol.Construction;
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
    public class ConstructionDataAccessor
    {
        public static RequestResult GetQueryFormModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new QueryFormModel()
                {
                    ConstructionFirmSelectItemList = new List<SelectListItem>() 
                    {
                        Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                    },
                    ConstructionTypeSelectItemList = new List<SelectListItem>() 
                    {
                        Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                    }
                };

                using (PDbEntities db = new PDbEntities())
                {
                    var constructionFirmList = db.ConstructionFirm.OrderBy(x => x.ID).ToList();

                    foreach (var constructionFirm in constructionFirmList)
                    {
                        model.ConstructionFirmSelectItemList.Add(new SelectListItem()
                        {
                            Text = constructionFirm.Name,
                            Value = constructionFirm.UniqueID
                        });
                    }

                    var constructionTypeList = db.ConstructionType.OrderBy(x => x.ID).ToList();

                    foreach (var constructionType in constructionTypeList)
                    {
                        model.ConstructionTypeSelectItemList.Add(new SelectListItem()
                        {
                            Text = constructionType.Description,
                            Value = constructionType.UniqueID
                        });
                    }
                }

                model.ConstructionFirmSelectItemList.Add(new SelectListItem()
                {
                    Text = Resources.Resource.Other,
                    Value = Define.OTHER
                });

                model.ConstructionTypeSelectItemList.Add(new SelectListItem()
                {
                    Text = Resources.Resource.Other,
                    Value = Define.OTHER
                });

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var query = (from c in db.Construction
                                 join f in db.ConstructionFirm
                                 on c.ConstructionFirmUniqueID equals f.UniqueID into tmpFirm
                                 from f in tmpFirm.DefaultIfEmpty()
                                 join t in db.ConstructionType
                                 on c.ConstructionTypeUniqueID equals t.UniqueID into tmpType
                                 from t in tmpType.DefaultIfEmpty()
                                 select new
                                 {
                                     c.UniqueID,
                                     c.VHNO,
                                     c.Description,
                                     c.Address,
                                     c.BeginDate,
                                     c.EndDate,
                                     c.ConstructionFirmUniqueID,
                                     ConstructionFirmName = f != null ? f.Name : string.Empty,
                                     c.ConstructionFirmRemark,
                                     c.ConstructionTypeUniqueID,
                                     ConstructionTypeDescription = t != null ? t.Description : string.Empty,
                                     c.ConstructionTypeRemark,
                                     c.CreateUserID,
                                     c.CreateTime
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.ConstructionFirmUniqueID))
                    {
                        query = query.Where(x => x.ConstructionFirmUniqueID == Parameters.ConstructionFirmUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.ConstructionTypeUniqueID))
                    {
                        query = query.Where(x => x.ConstructionTypeUniqueID == Parameters.ConstructionTypeUniqueID);
                    }

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
                            BeginDate = DateTimeHelper.DateString2DateStringWithSeparator(x.BeginDate),
                            EndDate = DateTimeHelper.DateString2DateStringWithSeparator(x.EndDate),
                            ConstructionFirmUniqueID = x.ConstructionFirmUniqueID,
                            ConstructionFirmName = x.ConstructionFirmName,
                            ConstructionFirmRemark = x.ConstructionFirmRemark,
                            ConstructionTypeUniqueID = x.ConstructionTypeUniqueID,
                            ConstructionTypeDescription = x.ConstructionTypeDescription,
                            ConstructionTypeRemark = x.ConstructionTypeRemark,
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
                    var query = (from c in db.Construction
                                 join p in db.PipePoint
                                 on c.PipePointUniqueID equals p.UniqueID into tmpPipePoint
                                 from p in tmpPipePoint.DefaultIfEmpty()
                                 join f in db.ConstructionFirm
                                 on c.ConstructionFirmUniqueID equals f.UniqueID into tmpFirm
                                 from f in tmpFirm.DefaultIfEmpty()
                                 join t in db.ConstructionType
                                 on c.ConstructionTypeUniqueID equals t.UniqueID into tmpType
                                 from t in tmpType.DefaultIfEmpty()
                                 where c.UniqueID == UniqueID
                                 select new
                                 {
                                     c.UniqueID,
                                     c.VHNO,
                                     c.Description,
                                     c.Address,
                                     c.BeginDate,
                                     c.EndDate,
                                     c.LNG,
                                     c.LAT,
                                     PipePointLAT = p != null ? p.LAT : default(double?),
                                     PipePointLNG = p != null ? p.LNG : default(double?),
                                     c.ConstructionFirmUniqueID,
                                     ConstructionFirmName = f != null ? f.Name : string.Empty,
                                     c.ConstructionFirmRemark,
                                     c.ConstructionTypeUniqueID,
                                     ConstructionTypeDescription = t != null ? t.Description : string.Empty,
                                     c.ConstructionTypeRemark,
                                     c.CreateUserID,
                                     c.CreateTime
                                 }).First();

                    var model = new DetailViewModel()
                    {
                        VHNO = query.VHNO,
                        Description = query.Description,
                        BeginDate = DateTimeHelper.DateString2DateStringWithSeparator(query.BeginDate),
                        EndDate = DateTimeHelper.DateString2DateStringWithSeparator(query.EndDate),
                        Address = query.Address,
                        LNG = query.LNG,
                        LAT = query.LAT,
                        PipePointLAT = query.PipePointLAT,
                        PipePointLNG = query.PipePointLNG,
                        ConstructionFirmUniqueID = query.ConstructionFirmUniqueID,
                        ConstructionFirmName = query.ConstructionFirmName,
                        ConstructionFirmRemark = query.ConstructionFirmRemark,
                        ConstructionTypeUniqueID = query.ConstructionTypeUniqueID,
                        ConstructionTypeDescription = query.ConstructionTypeDescription,
                        ConstructionTypeRemark = query.ConstructionTypeRemark,
                        CreateTime = query.CreateTime,
                        CreateUser = UserDataAccessor.GetUser(query.CreateUserID),
                        InspectionUserList = db.InspectionUser.Where(x => x.InspectionUniqueID == UniqueID).OrderBy(x => x.InspectTime).ToList().Select(x => new InspectionUserModel
                        {
                            InspectionTime = x.InspectTime,
                            Remark = x.Remark,
                            User = UserDataAccessor.GetUser(x.UserID)
                        }).ToList()
                    };

                    var photoList = db.ConstructionPhoto.Where(x => x.ConstructionUniqueID == UniqueID).OrderBy(x => x.Seq).ToList();

                    foreach (var photo in photoList)
                    {
                        model.PhotoList.Add(string.Format("{0}_{1}.{2}", photo.ConstructionUniqueID, photo.Seq, photo.Extension));
                    }

                    var dialog = db.Dialog.FirstOrDefault(x => x.ConstructionUniqueID == UniqueID);

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
