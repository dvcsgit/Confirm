using Customized.ECOVE.Models.AbnormalHandlingManagement;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using DataAccess;
using System.Transactions;

namespace Customized.ECOVE.DataAccess
{
    public class AbnormalHandlingDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                        if (Parameters.Status == "1" || Parameters.Status == "2" || Parameters.Status == "3" || Parameters.Status == "4")
                        {
                            var query1 = (from abnormal in db.Abnormal
                                          join x in db.AbnormalCheckResult
                                          on abnormal.UniqueID equals x.AbnormalUniqueID
                                          join c in db.CheckResult
                                          on x.CheckResultUniqueID equals c.UniqueID
                                          join y in db.RouteManager
                                          on c.RouteUniqueID equals y.RouteUniqueID
                                          join a in db.ArriveRecord
                                          on c.ArriveRecordUniqueID equals a.UniqueID
                                          join f in db.AbnormalRForm
                                          on abnormal.UniqueID equals f.AbnormalUniqueID into tmpR
                                          from f in tmpR.DefaultIfEmpty()
                                          where y.UserID == Account.ID
                                          select new
                                          {
                                              abnormal.UniqueID,
                                              c.RouteUniqueID,
                                              c.ControlPointID,
                                              c.ControlPointDescription,
                                              c.EquipmentUniqueID,
                                              c.EquipmentID,
                                              c.EquipmentName,
                                              c.PartDescription,
                                              c.CheckItemID,
                                              c.CheckItemDescription,
                                              c.CheckDate,
                                              c.CheckTime,
                                              c.IsAbnormal,
                                              c.IsAlert,
                                              abnormal.ClosedTime,
                                              abnormal.ClosedUserID,
                                              CheckUserID = a.UserID,
                                              VHNO = f!=null?f.RFormUniqueID:""
                                          }).Distinct().AsQueryable();

                            if (Parameters.Status == "1")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "2")
                            {
                                query1 = query1.Where(x => x.IsAlert && x.CheckDate == today);                               
                            }

                            if (Parameters.Status == "3")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "4")
                            {
                                query1 = query1.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }

                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID),
                                    VHNO = q.VHNO
                                };

                                var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                        }

                        if (Parameters.Status == "5" || Parameters.Status == "6" || Parameters.Status == "7" || Parameters.Status == "8")
                        {
                            var query1 = (from abnormal in db.Abnormal
                                          join x in db.AbnormalCheckResult
                                          on abnormal.UniqueID equals x.AbnormalUniqueID
                                          join c in db.CheckResult
                                          on x.CheckResultUniqueID equals c.UniqueID
                                          join r in db.Route
                                          on c.RouteUniqueID equals r.UniqueID
                                          join a in db.ArriveRecord
                                          on c.ArriveRecordUniqueID equals a.UniqueID
                                          join f in db.AbnormalRForm
                                         on abnormal.UniqueID equals f.AbnormalUniqueID into tmpR
                                          from f in tmpR.DefaultIfEmpty()
                                          where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                          select new
                                          {
                                              abnormal.UniqueID,
                                              c.RouteUniqueID,
                                              c.ControlPointID,
                                              c.ControlPointDescription,
                                              c.EquipmentUniqueID,
                                              c.EquipmentID,
                                              c.EquipmentName,
                                              c.PartDescription,
                                              c.CheckItemID,
                                              c.CheckItemDescription,
                                              c.CheckDate,
                                              c.CheckTime,
                                              c.IsAbnormal,
                                              c.IsAlert,
                                              abnormal.ClosedTime,
                                              abnormal.ClosedUserID,
                                              CheckUserID = a.UserID,
                                              VHNO = f != null ? f.RFormUniqueID : ""
                                          }).Distinct().AsQueryable();

                            if (Parameters.Status == "5")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "6")
                            {
                                query1 = query1.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "7")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "8")
                            {
                                query1 = query1.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }

                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID),
                                    VHNO = q.VHNO
                                };

                                var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                        }
                    }
                    else
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                        var query1 = (from abnormal in db.Abnormal
                                      join x in db.AbnormalCheckResult
                                      on abnormal.UniqueID equals x.AbnormalUniqueID
                                      join c in db.CheckResult
                                      on x.CheckResultUniqueID equals c.UniqueID
                                      join a in db.ArriveRecord
                                      on c.ArriveRecordUniqueID equals a.UniqueID
                                      join f in db.AbnormalRForm
                                       on abnormal.UniqueID equals f.AbnormalUniqueID into tmpR
                                      from f in tmpR.DefaultIfEmpty()
                                      where organizationList.Contains(c.OrganizationUniqueID)
                                      select new
                                      {
                                          abnormal.UniqueID,
                                          c.RouteUniqueID,
                                          c.ControlPointID,
                                          c.ControlPointDescription,
                                          c.EquipmentUniqueID,
                                          c.EquipmentID,
                                          c.EquipmentName,
                                          c.PartDescription,
                                          c.CheckItemID,
                                          c.CheckItemDescription,
                                          c.CheckDate,
                                          c.CheckTime,
                                          c.IsAbnormal,
                                          c.IsAlert,
                                          abnormal.ClosedTime,
                                          abnormal.ClosedUserID,
                                          CheckUserID = a.UserID,
                                          VHNO = f != null ? f.RFormUniqueID : ""
                                      }).AsQueryable();

                        if (!string.IsNullOrEmpty(Parameters.BeginDateString))
                        {
                            query1 = query1.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EndDateString))
                        {
                            query1 = query1.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                        {
                            query1 = query1.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                        }

                        if (!string.IsNullOrEmpty(Parameters.VHNO))
                        {
                            query1 = query1.Where(x => x.VHNO.Contains(Parameters.VHNO));
                        }

                        var queryResult = query1.ToList();

                        foreach (var q in queryResult)
                        {
                            var item = new GridItem
                            {
                                UniqueID = q.UniqueID,
                                ControlPointID = q.ControlPointID,
                                ControlPointDescription = q.ControlPointDescription,
                                EquipmentID = q.EquipmentID,
                                EquipmentName = q.EquipmentName,
                                PartDescription = q.PartDescription,
                                CheckItemID = q.CheckItemID,
                                CheckItemDescription = q.CheckItemDescription,
                                Date = q.CheckDate,
                                Time = q.CheckTime,
                                ClosedTime = q.ClosedTime,
                                IsAbnormal = q.IsAbnormal,
                                IsAlert = q.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID),
                                VHNO = q.VHNO
                            };

                            var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                            foreach (var routeManager in routeManagerList)
                            {
                                item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                            }

                            model.ItemList.Add(item);
                        }

                        model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                    }
                }

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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();



                using (EDbEntities edb = new EDbEntities())
                {
                    var query1 = (from abnormal in edb.Abnormal
                                  join x in edb.AbnormalCheckResult
                                  on abnormal.UniqueID equals x.AbnormalUniqueID
                                  join c in edb.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join a in edb.ArriveRecord
                                  on c.ArriveRecordUniqueID equals a.UniqueID
                                  join f in edb.AbnormalRForm
                                  on abnormal.UniqueID equals f.AbnormalUniqueID into tmpR
                                  from f in tmpR.DefaultIfEmpty()
                                  where abnormal.UniqueID == UniqueID
                                  select new
                                  {
                                      a.OrganizationUniqueID,
                                      abnormal.UniqueID,
                                      CheckResultUniqueID = c.UniqueID,
                                      c.RouteUniqueID,
                                      c.ControlPointID,
                                      c.ControlPointDescription,
                                      c.EquipmentUniqueID,
                                      c.EquipmentID,
                                      c.EquipmentName,
                                      c.PartDescription,
                                      c.CheckItemID,
                                      c.CheckItemDescription,
                                      c.CheckDate,
                                      c.CheckTime,
                                      c.IsAbnormal,
                                      c.IsAlert,
                                      c.Result,
                                      c.LowerLimit,
                                      c.LowerAlertLimit,
                                      c.UpperAlertLimit,
                                      c.UpperLimit,
                                      c.Unit,
                                      abnormal.ClosedTime,
                                      abnormal.ClosedUserID,
                                      abnormal.ClosedRemark,
                                      CheckUserID = a.UserID,
                                      VHNO =f!=null?f.RFormUniqueID:""
                                  }).First();

                    var factoryID = string.Empty;

                    using (DbEntities db = new DbEntities())
                    {
                        var organization = db.Organization.First(x => x.UniqueID == query1.OrganizationUniqueID);

                        var parentOrganization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                        while (parentOrganization.ID != "ECOVE")
                        {
                            organization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                            parentOrganization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);
                        }

                        factoryID = organization.ID;
                    }

                    model = new DetailViewModel()
                    {
                        UniqueID = query1.UniqueID,
                        FactoryID = factoryID,
                        VHNO = query1.VHNO,
                        ControlPointID = query1.ControlPointID,
                        ControlPointDescription = query1.ControlPointDescription,
                        EquipmentID = query1.EquipmentID,
                        EquipmentName = query1.EquipmentName,
                        PartDescription = query1.PartDescription,
                        CheckItemID = query1.CheckItemID,
                        CheckItemDescription = query1.CheckItemDescription,
                        Date = query1.CheckDate,
                        Time = query1.CheckTime,
                        IsAbnormal = query1.IsAbnormal,
                        IsAlert = query1.IsAlert,
                        Result = query1.Result,
                        LowerLimit = query1.LowerLimit,
                        LowerAlertLimit = query1.LowerAlertLimit,
                        UpperAlertLimit = query1.UpperAlertLimit,
                        UpperLimit = query1.UpperLimit,
                        Unit = query1.Unit,
                        CheckUser = UserDataAccessor.GetUser(query1.CheckUserID),
                        ClosedUser = UserDataAccessor.GetUser(query1.ClosedUserID),
                        ClosedTime = query1.ClosedTime,
                        ClosedRemark = query1.ClosedRemark,
                        AbnormalReasonList = edb.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == query1.CheckResultUniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                        {
                            Description = a.AbnormalReasonDescription,
                            Remark = a.AbnormalReasonRemark,
                            HandlingMethodList = edb.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == query1.CheckResultUniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                            {
                                Description = h.HandlingMethodDescription,
                                Remark = h.HandlingMethodRemark
                            }).ToList()
                        }).ToList(),
                        BeforePhotoList = edb.AbnormalPhoto.Where(p => p.AbnormalUniqueID == query1.UniqueID && p.Type == "B").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList(),
                        AfterPhotoList = edb.AbnormalPhoto.Where(p => p.AbnormalUniqueID == query1.UniqueID && p.Type == "A").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList(),
                        FileList = edb.AbnormalFile.Where(f => f.AbnormalUniqueID == query1.UniqueID).ToList().Select(f => new FileModel
                        {
                            AbnormalUniqueID = f.AbnormalUniqueID,
                            Seq = f.Seq,
                            FileName = f.FileName,
                            Extension = f.Extension,
                            Size = f.ContentLength,
                            UploadTime = f.UploadTime,
                            IsSaved = true
                        }).OrderBy(f => f.UploadTime).ToList()
                    };

                    var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == query1.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var routeManager in routeManagerList)
                    {
                        model.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                    }
                }

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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (EDbEntities edb = new EDbEntities())
                {
                    var query1 = (from abnormal in edb.Abnormal
                                  join x in edb.AbnormalCheckResult
                                  on abnormal.UniqueID equals x.AbnormalUniqueID
                                  join c in edb.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join a in edb.ArriveRecord
                                  on c.ArriveRecordUniqueID equals a.UniqueID
                                  join f in edb.AbnormalRForm
                                     on abnormal.UniqueID equals f.AbnormalUniqueID into tmpR
                                  from f in tmpR.DefaultIfEmpty()
                                  where abnormal.UniqueID == UniqueID
                                  select new
                                  {
                                      a.OrganizationUniqueID,
                                      abnormal.UniqueID,
                                      a.RouteID,
                                      CheckResultUniqueID = c.UniqueID,
                                      c.RouteUniqueID,
                                      c.ControlPointID,
                                      c.ControlPointDescription,
                                      c.EquipmentUniqueID,
                                      c.EquipmentID,
                                      c.EquipmentName,
                                      c.PartDescription,
                                      c.CheckItemID,
                                      c.CheckItemDescription,
                                      c.CheckDate,
                                      c.CheckTime,
                                      c.IsAbnormal,
                                      c.IsAlert,
                                      c.Result,
                                      c.LowerLimit,
                                      c.LowerAlertLimit,
                                      c.UpperAlertLimit,
                                      c.UpperLimit,
                                      c.Unit,
                                      abnormal.ClosedTime,
                                      abnormal.ClosedUserID,
                                      abnormal.ClosedRemark,
                                      CheckUserID = a.UserID,
                                      VHNO =f!=null?f.RFormUniqueID:""
                                  }).First();

                    var factoryID = string.Empty;

                    using (DbEntities db = new DbEntities())
                    {
                        var organization = db.Organization.First(x => x.UniqueID == query1.OrganizationUniqueID);

                        var parentOrganization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                        while (parentOrganization.ID != "ECOVE")
                        {
                            organization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                            parentOrganization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);
                        }

                        factoryID = organization.ID;
                    }

                    model = new EditFormModel()
                    {
                        CheckResultUniqueID = query1.CheckResultUniqueID,
                        UniqueID = query1.UniqueID,
                        RouteID = query1.RouteID,
                        FactoryID = factoryID,
                        VHNO=query1.VHNO,
                        ControlPointID = query1.ControlPointID,
                        ControlPointDescription = query1.ControlPointDescription,
                        EquipmentID = query1.EquipmentID,
                        EquipmentName = query1.EquipmentName,
                        PartDescription = query1.PartDescription,
                        CheckItemID = query1.CheckItemID,
                        CheckItemDescription = query1.CheckItemDescription,
                        Date = query1.CheckDate,
                        Time = query1.CheckTime,
                        IsAbnormal = query1.IsAbnormal,
                        IsAlert = query1.IsAlert,
                        Result = query1.Result,
                        LowerLimit = query1.LowerLimit,
                        LowerAlertLimit = query1.LowerAlertLimit,
                        UpperAlertLimit = query1.UpperAlertLimit,
                        UpperLimit = query1.UpperLimit,
                        Unit = query1.Unit,
                        CheckUser = UserDataAccessor.GetUser(query1.CheckUserID),
                        ClosedUser = UserDataAccessor.GetUser(query1.ClosedUserID),
                        ClosedTime = query1.ClosedTime,
                        ClosedRemark = query1.ClosedRemark,
                        AbnormalReasonList = edb.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == query1.CheckResultUniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                        {
                            Description = a.AbnormalReasonDescription,
                            Remark = a.AbnormalReasonRemark,
                            HandlingMethodList = edb.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == query1.CheckResultUniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                            {
                                Description = h.HandlingMethodDescription,
                                Remark = h.HandlingMethodRemark
                            }).ToList()
                        }).ToList()
                    };

                    var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == query1.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var routeManager in routeManagerList)
                    {
                        model.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                    }

                    var beforePhotoList = edb.AbnormalPhoto.Where(x => x.AbnormalUniqueID == model.UniqueID && x.Type == "B").OrderBy(x => x.Seq).ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        model.BeforePhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.AbnormalUniqueID,
                            Seq = photo.Seq,
                            Extension = photo.Extension,
                            Type = photo.Type,
                            IsSaved = true
                        });
                    }

                    var afterPhotoList = edb.AbnormalPhoto.Where(x => x.AbnormalUniqueID == model.UniqueID && x.Type == "A").OrderBy(x => x.Seq).ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        model.AfterPhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.AbnormalUniqueID,
                            Seq = photo.Seq,
                            Extension = photo.Extension,
                            Type = photo.Type,
                            IsSaved = true
                        });
                    }

                    model.FileList = edb.AbnormalFile.Where(x => x.AbnormalUniqueID == model.UniqueID).ToList().Select(x => new FileModel
                    {
                        AbnormalUniqueID = x.AbnormalUniqueID,
                        Seq = x.Seq,
                        FileName = x.FileName,
                        Extension = x.Extension,
                        Size = x.ContentLength,
                        UploadTime = x.UploadTime,
                        IsSaved = true
                    }).OrderBy(x => x.UploadTime).ToList();
                }

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

        public static RequestResult Save(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    using(TransactionScope trans = new TransactionScope())
                    {
                        var abnormal = db.Abnormal.First(x => x.UniqueID == Model.UniqueID);

                        abnormal.ClosedRemark = Model.ClosedRemark;

                        db.SaveChanges();

                        if (!string.IsNullOrEmpty(Model.VHNO))
                        {
                            db.AbnormalRForm.RemoveRange(db.AbnormalRForm.Where(x => x.AbnormalUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();

                            db.AbnormalRForm.Add(new AbnormalRForm()
                            {
                                AbnormalUniqueID = Model.UniqueID,
                                RFormUniqueID = Model.VHNO
                            });

                            db.SaveChanges();
                        }

                        var beforePhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == Model.UniqueID && x.Type == "B").ToList();

                        foreach (var photo in beforePhotoList)
                        {
                            if (!Model.BeforePhotoList.Any(x => x.Seq == photo.Seq))
                            {
                                db.AbnormalPhoto.Remove(photo);

                                try
                                {
                                    System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                                }
                                catch { }
                            }
                        }

                        var afterPhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == Model.UniqueID && x.Type == "A").ToList();

                        foreach (var photo in afterPhotoList)
                        {
                            if (!Model.AfterPhotoList.Any(x => x.Seq == photo.Seq))
                            {
                                db.AbnormalPhoto.Remove(photo);

                                try
                                {
                                    System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                                }
                                catch { }
                            }
                        }

                        foreach (var photo in Model.BeforePhotoList)
                        {
                            if (!photo.IsSaved)
                            {
                                db.AbnormalPhoto.Add(new AbnormalPhoto()
                                {
                                    AbnormalUniqueID = Model.UniqueID,
                                    Seq = photo.Seq,
                                    Extension = photo.Extension,
                                    Type = photo.Type
                                });

                                System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                            }
                        }

                        foreach (var photo in Model.AfterPhotoList)
                        {
                            if (!photo.IsSaved)
                            {
                                db.AbnormalPhoto.Add(new AbnormalPhoto()
                                {
                                    AbnormalUniqueID = Model.UniqueID,
                                    Seq = photo.Seq,
                                    Extension = photo.Extension,
                                    Type = photo.Type
                                });

                                System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                            }
                        }

                        var fileList = db.AbnormalFile.Where(x => x.AbnormalUniqueID == Model.UniqueID).ToList();

                        foreach (var file in fileList)
                        {
                            if (!Model.FileList.Any(x => x.Seq == file.Seq))
                            {
                                db.AbnormalFile.Remove(file);

                                try
                                {
                                    System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)));
                                }
                                catch { }
                            }
                        }

                        foreach (var file in Model.FileList)
                        {
                            if (!file.IsSaved)
                            {
                                db.AbnormalFile.Add(new AbnormalFile()
                                {
                                    AbnormalUniqueID = Model.UniqueID,
                                    Seq = file.Seq,
                                    Extension = file.Extension,
                                    FileName = file.FileName,
                                    UploadTime = file.UploadTime,
                                    ContentLength = file.Size
                                });

                                System.IO.File.Move(file.TempFullFileName, Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)));
                            }
                        }

                        db.SaveChanges();

                        trans.Complete();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Save, Resources.Resource.Success));   
                    }
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

        public static RequestResult Closed(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Save(Model);

                if (result.IsSuccess)
                {
                    if (string.IsNullOrEmpty(Model.ClosedRemark) && string.IsNullOrEmpty(Model.VHNO))
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1} {2}", Resources.Resource.CommentRequired, Resources.Resource.Or, "輸入工單編號"));
                    }
                    else
                    {
                        using (EDbEntities db = new EDbEntities())
                        {
                            var abnormal = db.Abnormal.First(x => x.UniqueID == Model.UniqueID);

                            abnormal.ClosedTime = DateTime.Now;
                            abnormal.ClosedUserID = Account.ID;

                            db.SaveChanges();
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));
                    }
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
