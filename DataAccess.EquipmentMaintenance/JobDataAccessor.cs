using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.EquipmentMaintenance.JobManagement;
using Models.Authenticated;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class JobDataAccessor
    {
        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from j in db.Job
                                 join r in db.Route
                                 on j.RouteUniqueID equals r.UniqueID
                                 where j.UniqueID == UniqueID
                                 select new
                                 {
                                     UniqueID = j.UniqueID,
                                     OrganizationUniqueID = r.OrganizationUniqueID,
                                     RouteUniqueID = r.UniqueID,
                                     RouteID=  r.ID,
                                     RouteName=r.Name,
                                    JobDescription= j.Description,
                                    j.IsCheckBySeq,
                                    j.IsShowPrevRecord,
                                    j.IsNeedVerify,
                                    j.BeginDate,
                                    j.EndDate,
                                    j.BeginTime,
                                    j.EndTime,
                                    j.TimeMode,
                                    j.CycleCount,
                                    j.CycleMode,
                                    j.Remark
                                 }).First();

                    model = new DetailViewModel
                    {
                        UniqueID = query.UniqueID,
                        Permission = Account.OrganizationPermission(query.OrganizationUniqueID),
                        RouteUniqueID = query.RouteUniqueID,
                        RouteID = query.RouteID,
                        RouteName = query.RouteName,
                        Description = query.JobDescription,
                        IsCheckBySeq = query.IsCheckBySeq,
                        IsShowPrevRecord = query.IsShowPrevRecord,
                        IsNeedVerify = query.IsNeedVerify,
                        BeginDate = query.BeginDate,
                        EndDate = query.EndDate,
                        TimeMode = query.TimeMode == 0 ? Resources.Resource.TimeMode0 : Resources.Resource.TimeMode1,
                        BeginTime = query.BeginTime,
                        EndTime = query.EndTime,
                        CycleCount = query.CycleCount,
                        CycleMode = query.CycleMode,
                        Remark = query.Remark,
                        UserList = db.JobUser.Where(x => x.JobUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.JobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };
                }

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.EquipmentMaintenance.JobManagement.UserModel
                                      {
                                          OrganizationDescription = o.Description,
                                          ID = u.ID,
                                          Name = u.Name
                                      }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList();
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

        public static RequestResult GetCreateFormModel(string RouteUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == RouteUniqueID);

                    result.ReturnData(new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(route.OrganizationUniqueID),
                        RouteUniqueID = RouteUniqueID,
                        RouteID = route.ID,
                        RouteName = route.Name,
                        ControlPointList = (from rc in db.RouteControlPoint
                                            join controlPoint in db.ControlPoint
                                            on rc.ControlPointUniqueID equals controlPoint.UniqueID
                                            where rc.RouteUniqueID == route.UniqueID
                                            select new ControlPointModel
                                            {
                                                UniqueID = controlPoint.UniqueID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.Description,
                                                MinTimeSpan =rc.MinTimeSpan,
                                                Seq = rc.Seq,
                                                CheckItemList = (from x in db.RouteControlPointCheckItem
                                                                 join c in db.CheckItem
                                                                 on x.CheckItemUniqueID equals c.UniqueID
                                                                 where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new CheckItemModel
                                                                 {
                                                                     IsChecked = true,
                                                                     UniqueID = c.UniqueID,
                                                                     CheckType = c.CheckType,
                                                                     CheckItemID = c.ID,
                                                                     CheckItemDescription = c.Description,
                                                                     Seq = x.Seq
                                                                 }).OrderBy(x => x.Seq).ToList(),
                                                EquipmentList = (from re in db.RouteEquipment
                                                                 join equipment in db.Equipment
                                                                 on re.EquipmentUniqueID equals equipment.UniqueID
                                                                 join part in db.EquipmentPart
                                                                 on re.PartUniqueID equals part.UniqueID into tmpPart
                                                                 from part in tmpPart.DefaultIfEmpty()
                                                                 where re.RouteUniqueID == route.UniqueID && re.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new EquipmentModel
                                                                 {
                                                                     EquipmentUniqueID = equipment.UniqueID,
                                                                     PartUniqueID = part != null ? part.UniqueID : "*",
                                                                     EquipmentID = equipment.ID,
                                                                     EquipmentName = equipment.Name,
                                                                     PartDescription = part != null ? part.Description : "",
                                                                     Seq = re.Seq,
                                                                     CheckItemList = (from x in db.RouteEquipmentCheckItem
                                                                                      join c in db.CheckItem
                                                                                      on x.CheckItemUniqueID equals c.UniqueID
                                                                                      where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == re.EquipmentUniqueID && x.PartUniqueID == re.PartUniqueID
                                                                                      select new CheckItemModel
                                                                                      {
                                                                                          IsChecked = true,
                                                                                          UniqueID = c.UniqueID,
                                                                                          CheckType = c.CheckType,
                                                                                          CheckItemID = c.ID,
                                                                                          CheckItemDescription = c.Description,
                                                                                          Seq = x.Seq
                                                                                      }).OrderBy(x => x.Seq).ToList()
                                                                 }).OrderBy(x => x.Seq).ToList()
                                            }).OrderBy(x => x.Seq).ToList()
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var job = (from j in db.Job
                               join r in db.Route
                               on j.RouteUniqueID equals r.UniqueID
                               where j.UniqueID == UniqueID
                               select new { Job = j, Route = r }).First();

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.Route.OrganizationUniqueID),
                        RouteUniqueID = job.Route.UniqueID,
                        RouteID = job.Route.ID,
                        RouteName = job.Route.Name,
                        FormInput = new FormInput()
                        {
                            IsCheckBySeq = job.Job.IsCheckBySeq,
                            IsNeedVerify = job.Job.IsNeedVerify,
                            IsShowPrevRecord = job.Job.IsShowPrevRecord,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.BeginDate),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.EndDate),
                            TimeMode = job.Job.TimeMode,
                            BeginTime = job.Job.BeginTime,
                            EndTime = job.Job.EndTime,
                            CycleCount = job.Job.CycleCount,
                            CycleMode = job.Job.CycleMode,
                            Remark = job.Job.Remark
                        },
                        UserList = db.JobUser.Where(x => x.JobUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.JobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };

                    var jobControlPointList = db.JobControlPoint.Where(x => x.JobUniqueID == UniqueID).ToList();
                    var jobControlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == UniqueID).ToList();
                    var jobEquipmentList = db.JobEquipment.Where(x => x.JobUniqueID == UniqueID).ToList();
                    var jobEquipmentCheckItemList = db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == UniqueID).ToList();

                    var routeControlPointList = (from x in db.RouteControlPoint
                                                 join c in db.ControlPoint
                                                 on x.ControlPointUniqueID equals c.UniqueID
                                                 where x.RouteUniqueID == model.RouteUniqueID
                                                 select new
                                                 {
                                                     c.UniqueID,
                                                     c.ID,
                                                     c.Description,
                                                     x.Seq
                                                 }).ToList();

                    foreach (var routeControlPoint in routeControlPointList)
                    {
                        var jobControlPoint = jobControlPointList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID);

                        var controlPointModel = new ControlPointModel()
                        {
                            UniqueID = routeControlPoint.UniqueID,
                            ID = routeControlPoint.ID,
                            Description = routeControlPoint.Description,
                            MinTimeSpan = jobControlPoint != null ? jobControlPoint.MinTimeSpan : default(int?),
                            Seq = routeControlPoint.Seq
                        };

                        var routeControlPointCheckItemList = (from x in db.RouteControlPointCheckItem
                                                              join c in db.CheckItem
                                                              on x.CheckItemUniqueID equals c.UniqueID
                                                              where x.RouteUniqueID == model.RouteUniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID
                                                              select new
                                                              {
                                                                  c.UniqueID,
                                                                  c.CheckType,
                                                                  c.ID,
                                                                  c.Description,
                                                                  x.Seq
                                                              }).ToList();

                        foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                        {
                            var jobControlPointCheckItem = jobControlPointCheckItemList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.CheckItemUniqueID == routeControlPointCheckItem.UniqueID);

                            controlPointModel.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = jobControlPointCheckItem != null,
                                UniqueID = routeControlPointCheckItem.UniqueID,
                                CheckType = routeControlPointCheckItem.CheckType,
                                CheckItemID = routeControlPointCheckItem.ID,
                                CheckItemDescription = routeControlPointCheckItem.Description,
                                Seq = routeControlPointCheckItem.Seq
                            });
                        }

                        controlPointModel.CheckItemList = controlPointModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                        var routeEquipmntList = (from x in db.RouteEquipment
                                                 join e in db.Equipment
                                                 on x.EquipmentUniqueID equals e.UniqueID
                                                 join p in db.EquipmentPart
                                                 on x.PartUniqueID equals p.UniqueID into tmpPart
                                                 from p in tmpPart.DefaultIfEmpty()
                                                 where x.RouteUniqueID == model.RouteUniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UniqueID,
                                                     PartUniqueID = p != null ? p.UniqueID : "*",
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.Name,
                                                     PartDescription = p != null ? p.Description : "",
                                                     x.Seq
                                                 }).ToList();

                        foreach (var routeEquipment in routeEquipmntList)
                        {
                            var jobEquipment = jobEquipmentList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.EquipmentUniqueID == routeEquipment.EquipmentUniqueID && x.PartUniqueID == routeEquipment.PartUniqueID);

                            var equipmentModel = new EquipmentModel()
                            {
                                EquipmentUniqueID = routeEquipment.EquipmentUniqueID,
                                PartUniqueID = routeEquipment.PartUniqueID,
                                EquipmentID = routeEquipment.EquipmentID,
                                EquipmentName = routeEquipment.EquipmentName,
                                PartDescription = routeEquipment.PartDescription,
                                Seq = routeEquipment.Seq
                            };

                            var routeEquipmentCheckItemList = (from x in db.RouteEquipmentCheckItem
                                                               join c in db.CheckItem
                                                               on x.CheckItemUniqueID equals c.UniqueID
                                                               where x.RouteUniqueID == model.RouteUniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID && x.EquipmentUniqueID == routeEquipment.EquipmentUniqueID && x.PartUniqueID == routeEquipment.PartUniqueID
                                                               select new
                                                               {
                                                                   c.UniqueID,
                                                                   c.CheckType,
                                                                   c.ID,
                                                                   c.Description,
                                                                   x.Seq
                                                               }).ToList();

                            foreach (var routeEquipmentCheckItem in routeEquipmentCheckItemList)
                            {
                                var jobEquipmentCheckItem = jobEquipmentCheckItemList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.EquipmentUniqueID == routeEquipment.EquipmentUniqueID && x.PartUniqueID == routeEquipment.PartUniqueID && x.CheckItemUniqueID == routeEquipmentCheckItem.UniqueID);

                                equipmentModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = jobEquipmentCheckItem != null,
                                    UniqueID = routeEquipmentCheckItem.UniqueID,
                                    CheckType = routeEquipmentCheckItem.CheckType,
                                    CheckItemID = routeEquipmentCheckItem.ID,
                                    CheckItemDescription = routeEquipmentCheckItem.Description,
                                    Seq = routeEquipmentCheckItem.Seq
                                });
                            }

                            equipmentModel.CheckItemList = equipmentModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                            controlPointModel.EquipmentList.Add(equipmentModel);
                        }

                        controlPointModel.EquipmentList = controlPointModel.EquipmentList.OrderBy(x => x.Seq).ToList();

                        model.ControlPointList.Add(controlPointModel);
                    }
                }

                model.ControlPointList = model.ControlPointList.OrderBy(x => x.Seq).ToList();

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.EquipmentMaintenance.JobManagement.UserModel
                                      {
                                          OrganizationDescription = o.Description,
                                          ID = u.ID,
                                          Name = u.Name
                                      }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList();
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var exists = db.Job.FirstOrDefault(x => x.RouteUniqueID == Model.RouteUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.Job.Add(new Job()
                        {
                            UniqueID = uniqueID,
                            RouteUniqueID = Model.RouteUniqueID,
                            Description = Model.FormInput.Description,
                            TimeMode = Model.FormInput.TimeMode,
                            BeginTime = Model.FormInput.BeginTime,
                            EndTime = Model.FormInput.EndTime,
                            IsCheckBySeq = Model.FormInput.IsCheckBySeq,
                            IsShowPrevRecord = Model.FormInput.IsShowPrevRecord,
                            IsNeedVerify = Model.FormInput.IsNeedVerify,
                            CycleCount = Model.FormInput.CycleCount,
                            CycleMode = Model.FormInput.CycleMode,
                            BeginDate = Model.FormInput.BeginDate,
                            EndDate = Model.FormInput.EndDate,
                            Remark = Model.FormInput.Remark,
                            LastModifyTime = DateTime.Now
                        });

                        db.JobUser.AddRange(Model.UserList.Select(x => new JobUser
                        {
                            JobUniqueID = uniqueID,
                            UserID = x.ID
                        })).ToList();

                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            if (controlPoint.CheckItemList.Count > 0 || controlPoint.EquipmentList.Any(x => x.CheckItemList.Count(c => c.IsChecked) > 0))
                            {
                                db.JobControlPoint.Add(new JobControlPoint()
                                {
                                    JobUniqueID = uniqueID,
                                    ControlPointUniqueID = controlPoint.UniqueID,
                                    MinTimeSpan = controlPoint.MinTimeSpan
                                });

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.JobControlPointCheckItem.Add(new JobControlPointCheckItem()
                                        {
                                            JobUniqueID = uniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            CheckItemUniqueID = checkItem.UniqueID
                                        });
                                    }
                                }

                                foreach (var equipment in controlPoint.EquipmentList)
                                {
                                    if (equipment.CheckItemList.Count(x => x.IsChecked) > 0)
                                    {
                                        db.JobEquipment.Add(new JobEquipment()
                                        {
                                            JobUniqueID = uniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            EquipmentUniqueID = equipment.EquipmentUniqueID,
                                            PartUniqueID = equipment.PartUniqueID
                                        });

                                        foreach (var checkItem in equipment.CheckItemList)
                                        {
                                            if (checkItem.IsChecked)
                                            {
                                                db.JobEquipmentCheckItem.Add(new JobEquipmentCheckItem()
                                                {
                                                    JobUniqueID = uniqueID,
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                                    PartUniqueID = equipment.PartUniqueID,
                                                    CheckItemUniqueID = checkItem.UniqueID
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Job, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.JobDescription, Resources.Resource.Exists));
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var job = (from j in db.Job
                               join r in db.Route
                               on j.RouteUniqueID equals r.UniqueID
                               where j.UniqueID == UniqueID
                               select new { Job = j, Route = r }).First();

                    model = new EditFormModel()
                    {
                        UniqueID = job.Job.UniqueID,
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.Route.OrganizationUniqueID),
                        RouteUniqueID = job.Route.UniqueID,
                        RouteID = job.Route.ID,
                        RouteName = job.Route.Name,
                        FormInput = new FormInput()
                        {
                            Description = job.Job.Description,
                            IsCheckBySeq = job.Job.IsCheckBySeq,
                            IsNeedVerify = job.Job.IsNeedVerify,
                            IsShowPrevRecord = job.Job.IsShowPrevRecord,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.BeginDate),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.EndDate),
                            TimeMode = job.Job.TimeMode,
                            BeginTime = job.Job.BeginTime,
                            EndTime = job.Job.EndTime,
                            CycleCount = job.Job.CycleCount,
                            CycleMode = job.Job.CycleMode,
                            Remark = job.Job.Remark
                        },
                        UserList = db.JobUser.Where(x => x.JobUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.JobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };

                    var jobControlPointList = db.JobControlPoint.Where(x => x.JobUniqueID == UniqueID).ToList();
                    var jobControlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == UniqueID).ToList();
                    var jobEquipmentList = db.JobEquipment.Where(x => x.JobUniqueID == UniqueID).ToList();
                    var jobEquipmentCheckItemList = db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == UniqueID).ToList();

                    var routeControlPointList = (from x in db.RouteControlPoint
                                                 join c in db.ControlPoint
                                                 on x.ControlPointUniqueID equals c.UniqueID
                                                 where x.RouteUniqueID == model.RouteUniqueID
                                                 select new
                                                 {
                                                     c.UniqueID,
                                                     c.ID,
                                                     c.Description,
                                                     x.Seq
                                                 }).ToList();

                    foreach (var routeControlPoint in routeControlPointList)
                    {
                        var jobControlPoint = jobControlPointList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID);

                        var controlPointModel = new ControlPointModel()
                        {
                            UniqueID = routeControlPoint.UniqueID,
                            ID = routeControlPoint.ID,
                            Description = routeControlPoint.Description,
                            MinTimeSpan = jobControlPoint != null ? jobControlPoint.MinTimeSpan : default(int?),
                            Seq = routeControlPoint.Seq
                        };

                        var routeControlPointCheckItemList = (from x in db.RouteControlPointCheckItem
                                                              join c in db.CheckItem
                                                              on x.CheckItemUniqueID equals c.UniqueID
                                                              where x.RouteUniqueID == model.RouteUniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID
                                                              select new
                                                              {
                                                                  c.UniqueID,
                                                                  c.CheckType,
                                                                  c.ID,
                                                                  c.Description,
                                                                  x.Seq
                                                              }).ToList();

                        foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                        {
                            var jobControlPointCheckItem = jobControlPointCheckItemList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.CheckItemUniqueID == routeControlPointCheckItem.UniqueID);

                            controlPointModel.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = jobControlPointCheckItem != null,
                                UniqueID = routeControlPointCheckItem.UniqueID,
                                CheckType = routeControlPointCheckItem.CheckType,
                                CheckItemID = routeControlPointCheckItem.ID,
                                CheckItemDescription = routeControlPointCheckItem.Description,
                                Seq = routeControlPointCheckItem.Seq
                            });
                        }

                        controlPointModel.CheckItemList = controlPointModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                        var routeEquipmntList = (from x in db.RouteEquipment
                                                 join e in db.Equipment
                                                 on x.EquipmentUniqueID equals e.UniqueID
                                                 join p in db.EquipmentPart
                                                 on x.PartUniqueID equals p.UniqueID into tmpPart
                                                 from p in tmpPart.DefaultIfEmpty()
                                                 where x.RouteUniqueID == model.RouteUniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UniqueID,
                                                     PartUniqueID = p != null ? p.UniqueID : "*",
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.Name,
                                                     PartDescription = p != null ? p.Description : "",
                                                     x.Seq
                                                 }).ToList();

                        foreach (var routeEquipment in routeEquipmntList)
                        {
                            var jobEquipment = jobEquipmentList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.EquipmentUniqueID == routeEquipment.EquipmentUniqueID && x.PartUniqueID == routeEquipment.PartUniqueID);

                            var equipmentModel = new EquipmentModel()
                            {
                                EquipmentUniqueID = routeEquipment.EquipmentUniqueID,
                                PartUniqueID = routeEquipment.PartUniqueID,
                                EquipmentID = routeEquipment.EquipmentID,
                                EquipmentName = routeEquipment.EquipmentName,
                                PartDescription = routeEquipment.PartDescription,
                                Seq = routeEquipment.Seq
                            };

                            var routeEquipmentCheckItemList = (from x in db.RouteEquipmentCheckItem
                                                               join c in db.CheckItem
                                                               on x.CheckItemUniqueID equals c.UniqueID
                                                               where x.RouteUniqueID == model.RouteUniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID && x.EquipmentUniqueID == routeEquipment.EquipmentUniqueID && x.PartUniqueID == routeEquipment.PartUniqueID
                                                               select new
                                                               {
                                                                   c.UniqueID,
                                                                   c.CheckType,
                                                                   c.ID,
                                                                   c.Description,
                                                                   x.Seq
                                                               }).ToList();

                            foreach (var routeEquipmentCheckItem in routeEquipmentCheckItemList)
                            {
                                var jobEquipmentCheckItem = jobEquipmentCheckItemList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.EquipmentUniqueID == routeEquipment.EquipmentUniqueID && x.PartUniqueID == routeEquipment.PartUniqueID && x.CheckItemUniqueID == routeEquipmentCheckItem.UniqueID);

                                equipmentModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = jobEquipmentCheckItem != null,
                                    UniqueID = routeEquipmentCheckItem.UniqueID,
                                    CheckType = routeEquipmentCheckItem.CheckType,
                                    CheckItemID = routeEquipmentCheckItem.ID,
                                    CheckItemDescription = routeEquipmentCheckItem.Description,
                                    Seq = routeEquipmentCheckItem.Seq
                                });
                            }

                            equipmentModel.CheckItemList = equipmentModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                            controlPointModel.EquipmentList.Add(equipmentModel);
                        }

                        controlPointModel.EquipmentList = controlPointModel.EquipmentList.OrderBy(x => x.Seq).ToList();

                        model.ControlPointList.Add(controlPointModel);
                    }
                }

                model.ControlPointList = model.ControlPointList.OrderBy(x => x.Seq).ToList();

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.EquipmentMaintenance.JobManagement.UserModel
                                      {
                                          OrganizationDescription = o.Description,
                                          ID = u.ID,
                                          Name = u.Name
                                      }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList();
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Job.FirstOrDefault(x => x.UniqueID != job.UniqueID && x.RouteUniqueID == job.RouteUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Job
                        job.Description = Model.FormInput.Description;
                        job.IsCheckBySeq = Model.FormInput.IsCheckBySeq;
                        job.IsNeedVerify = Model.FormInput.IsNeedVerify;
                        job.IsShowPrevRecord = Model.FormInput.IsShowPrevRecord;
                        job.BeginDate = Model.FormInput.BeginDate;
                        job.EndDate = Model.FormInput.EndDate;
                        job.TimeMode = Model.FormInput.TimeMode;
                        job.BeginTime = Model.FormInput.BeginTime;
                        job.EndTime = Model.FormInput.EndTime;
                        job.CycleCount = Model.FormInput.CycleCount;
                        job.CycleMode = Model.FormInput.CycleMode;
                        job.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region JobUser
                        #region Delete
                        db.JobUser.RemoveRange(db.JobUser.Where(x => x.JobUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.JobUser.AddRange(Model.UserList.Select(x => new JobUser
                        {
                            JobUniqueID = job.UniqueID,
                            UserID = x.ID
                        })).ToList();
                        #endregion
                        #endregion

                        #region JobControlPoint, JobControlPointCheckItem, JobEquipment, JobEquipmentCheckItem
                        #region Delete
                        db.JobControlPoint.RemoveRange(db.JobControlPoint.Where(x => x.JobUniqueID == job.UniqueID).ToList());
                        db.JobControlPointCheckItem.RemoveRange(db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.UniqueID).ToList());
                        db.JobEquipment.RemoveRange(db.JobEquipment.Where(x => x.JobUniqueID == job.UniqueID).ToList());
                        db.JobEquipmentCheckItem.RemoveRange(db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == job.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            if (controlPoint.CheckItemList.Count > 0 || controlPoint.EquipmentList.Any(x => x.CheckItemList.Count(c => c.IsChecked) > 0))
                            {
                                db.JobControlPoint.Add(new JobControlPoint()
                                {
                                    JobUniqueID = job.UniqueID,
                                    ControlPointUniqueID = controlPoint.UniqueID,
                                    MinTimeSpan = controlPoint.MinTimeSpan
                                });

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.JobControlPointCheckItem.Add(new JobControlPointCheckItem()
                                        {
                                            JobUniqueID = job.UniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            CheckItemUniqueID = checkItem.UniqueID
                                        });
                                    }
                                }

                                foreach (var equipment in controlPoint.EquipmentList)
                                {
                                    if (equipment.CheckItemList.Count(x => x.IsChecked) > 0)
                                    {
                                        db.JobEquipment.Add(new JobEquipment()
                                        {
                                            JobUniqueID = job.UniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            EquipmentUniqueID = equipment.EquipmentUniqueID,
                                            PartUniqueID = equipment.PartUniqueID
                                        });

                                        foreach (var checkItem in equipment.CheckItemList)
                                        {
                                            if (checkItem.IsChecked)
                                            {
                                                db.JobEquipmentCheckItem.Add(new JobEquipmentCheckItem()
                                                {
                                                    JobUniqueID = job.UniqueID,
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                                    PartUniqueID = equipment.PartUniqueID,
                                                    CheckItemUniqueID = checkItem.UniqueID
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Job, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.JobDescription, Resources.Resource.Exists));
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

        public static RequestResult Delete(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    DeleteHelper.Job(db, UniqueID);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Job, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<ControlPointModel> ControlPointList, List<string> ControlPointPageStateList, List<string> ControlPointCheckItemPageStateList, List<string> EquipmentCheckItemPageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string controlPointPageState in ControlPointPageStateList)
                {
                    string[] temp = controlPointPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string uniqueID = temp[0];
                    int? minTimeSpan = !string.IsNullOrEmpty(temp[1]) ? int.Parse(temp[1]) : default(int?);

                    var controlPoint = ControlPointList.First(x => x.UniqueID == uniqueID);

                    controlPoint.MinTimeSpan = minTimeSpan;
                }

                foreach (var controlPointCheckItemPageState in ControlPointCheckItemPageStateList)
                {
                    string[] temp = controlPointCheckItemPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string controlPointUniqueID = temp[0];
                    string checkItemUniqueID = temp[1];
                    string isChecked = temp[2];

                    var checkItem = ControlPointList.First(x => x.UniqueID == controlPointUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsChecked = isChecked == "Y";
                }

                foreach (var equipmentCheckItemPageState in EquipmentCheckItemPageStateList)
                {
                    string[] temp = equipmentCheckItemPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string controlPointUniqueID = temp[0];
                    string equipmentUniqueID = temp[1];
                    string partUniqueID = temp[2];
                    string checkItemUniqueID = temp[3];
                    string isChecked = temp[4];

                    var checkItem = ControlPointList.First(x => x.UniqueID == controlPointUniqueID).EquipmentList.First(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsChecked = isChecked == "Y";
                }

                result.ReturnData(ControlPointList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailTreeItem(string JobUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var controlPointList = (from x in db.JobControlPoint
                                            join j in db.Job
                                            on x.JobUniqueID equals j.UniqueID
                                            join y in db.RouteControlPoint
                                            on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.JobUniqueID == JobUniqueID
                                            select new
                                            {
                                                c.UniqueID,
                                                c.ID,
                                                c.Description,
                                                y.Seq
                                            }).OrderBy(x => x.Seq).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                        attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.JobEquipment.Any(x => x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID)
                            ||
                            db.JobControlPointCheckItem.Any(x => x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID))
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailTreeItem(string JobUniqueID, string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty },
                };

                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(PartUniqueID) && PartUniqueID!="*")
                    {
                        var checkItemList = (from x in db.JobEquipmentCheckItem
                                             join j in db.Job
                                             on x.JobUniqueID equals j.UniqueID
                                             join y in db.RouteEquipmentCheckItem
                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID}
                                             join c in db.CheckItem
                                             on x.CheckItemUniqueID equals c.UniqueID
                                             where x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == ControlPointUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID
                                             select new
                                             {
                                                 c.ID,
                                                 c.Description,
                                                 y.Seq
                                             }).OrderBy(x => x.Seq).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (!string.IsNullOrEmpty(EquipmentUniqueID))
                    {
                        var checkItemList = (from x in db.JobEquipmentCheckItem
                                             join j in db.Job
                                             on x.JobUniqueID equals j.UniqueID
                                             join y in db.RouteEquipmentCheckItem
                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                             join c in db.CheckItem
                                             on x.CheckItemUniqueID equals c.UniqueID
                                             where x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == ControlPointUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == "*"
                                             select new
                                             {
                                                 c.ID,
                                                 c.Description,
                                                 y.Seq
                                             }).OrderBy(x => x.Seq).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var checkItemList = (from x in db.JobControlPointCheckItem
                                             join j in db.Job
                                             on x.JobUniqueID equals j.UniqueID
                                             join y in db.RouteControlPointCheckItem
                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID}
                                             join c in db.CheckItem
                                             on x.CheckItemUniqueID equals c.UniqueID
                                             where x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == ControlPointUniqueID
                                             select new
                                             {
                                                 c.ID,
                                                 c.Description,
                                                 y.Seq
                                             }).OrderBy(x => x.Seq).ToList();


                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var equipmentList = (from x in db.JobEquipment
                                             join j in db.Job
                                             on x.JobUniqueID equals j.UniqueID
                                             join y in db.RouteEquipment
                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                             join e in db.Equipment
                                             on x.EquipmentUniqueID equals e.UniqueID
                                             join p in db.EquipmentPart
                                             on x.PartUniqueID equals p.UniqueID into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             where x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == ControlPointUniqueID
                                             select new
                                             {
                                                 EquipmentUniqueID = e.UniqueID,
                                                 PartUniqueID = p != null ? p.UniqueID : "*",
                                                 EquipmentID = e.ID,
                                                 EquipmentName = e.Name,
                                                 PartDescription = p != null ? p.Description : "",
                                                 y.Seq
                                             }).OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var treeItem = new TreeItem();

                            if (equipment.PartUniqueID == "*")
                            {
                                treeItem.Title = equipment.EquipmentName;
                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.EquipmentID, equipment.EquipmentName, equipment.PartDescription);
                            }
                            else
                            {
                                treeItem.Title = string.Format("{0}-{1}", equipment.EquipmentName, equipment.PartDescription);
                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.EquipmentID, equipment.EquipmentName);
                            }

                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = equipment.PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.JobEquipmentCheckItem.Any(x => x.JobUniqueID == JobUniqueID && x.ControlPointUniqueID == ControlPointUniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddUser(List<Models.EquipmentMaintenance.JobManagement.UserModel> UserList, List<string> SelectedList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                string[] seperator = new string[] { Define.Seperator };

                using (DbEntities db = new DbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(seperator, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var userID = temp[1];

                        if (!string.IsNullOrEmpty(userID))
                        {
                            if (!UserList.Any(x => x.ID == userID))
                            {
                                UserList.Add((from u in db.User
                                              join o in db.Organization
                                                  on u.OrganizationUniqueID equals o.UniqueID
                                              where u.ID == userID
                                              select new Models.EquipmentMaintenance.JobManagement.UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.Name,
                                                  OrganizationDescription = o.Description
                                              }).First());
                            }
                        }
                        else
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);
                            var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                            var userList = (from u in db.User
                                            join o in db.Organization
                                           on u.OrganizationUniqueID equals o.UniqueID
                                            where organizationList.Contains(u.OrganizationUniqueID)
                                            select new
                                            {
                                                ID = u.ID,
                                                Name = u.Name,
                                                OrganizationDescription = o.Description
                                            }).ToList();

                            foreach (var user in userList)
                            {
                                if (!UserList.Any(x => x.ID == user.ID))
                                {
                                    UserList.Add(new Models.EquipmentMaintenance.JobManagement.UserModel()
                                    {
                                        ID = user.ID,
                                        Name = user.Name,
                                        OrganizationDescription = user.OrganizationDescription
                                    });
                                }
                            }
                        }
                    }
                }

                result.ReturnData(UserList.OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
