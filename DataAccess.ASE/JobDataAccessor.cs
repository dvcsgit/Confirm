using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.EquipmentMaintenance.JobManagement;
using Models.Authenticated;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class JobDataAccessor
    {
        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from j in db.JOB
                                 join r in db.ROUTE
                                 on j.ROUTEUNIQUEID equals r.UNIQUEID
                                 where j.UNIQUEID == UniqueID
                                 select new
                                 {
                                     UniqueID = j.UNIQUEID,
                                     OrganizationUniqueID = r.ORGANIZATIONUNIQUEID,
                                     RouteUniqueID = r.UNIQUEID,
                                     RouteID=  r.ID,
                                     RouteName=r.NAME,
                                    JobDescription= j.DESCRIPTION,
                                     IsCheckBySeq=j.ISCHECKBYSEQ=="Y",
                                     IsShowPrevRecord=j.ISSHOWPREVRECORD=="Y",
                                     IsNeedVerify=j.ISNEEDVERIFY=="Y",
                                     BeginDate=j.BEGINDATE.Value,
                                     EndDate=j.ENDDATE,
                                     BeginTime=j.BEGINTIME,
                                     EndTime=j.ENDTIME,
                                     TimeMode=j.TIMEMODE,
                                     CycleCount=j.CYCLECOUNT.Value,
                                     CycleMode=j.CYCLEMODE,
                                     Remark=j.REMARK
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
                        UserList = db.JOBUSER.Where(x => x.JOBUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.JobManagement.UserModel
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    model.UserList = (from user in model.UserList
                                      join u in db.ACCOUNT
                                      on user.ID equals u.ID
                                      join o in db.ORGANIZATION
                                      on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                      select new Models.EquipmentMaintenance.JobManagement.UserModel
                                      {
                                          OrganizationDescription = o.DESCRIPTION,
                                          ID = u.ID,
                                          Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var route = db.ROUTE.First(x => x.UNIQUEID == RouteUniqueID);

                    var model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(route.ORGANIZATIONUNIQUEID),
                        RouteUniqueID = RouteUniqueID,
                        RouteID = route.ID,
                        RouteName = route.NAME,
                        ControlPointList = (from rc in db.ROUTECONTROLPOINT
                                            join controlPoint in db.CONTROLPOINT
                                            on rc.CONTROLPOINTUNIQUEID equals controlPoint.UNIQUEID
                                            where rc.ROUTEUNIQUEID == route.UNIQUEID
                                            select new ControlPointModel
                                            {
                                                UniqueID = controlPoint.UNIQUEID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.DESCRIPTION,
                                                MinTimeSpan = rc.MINTIMESPAN,
                                                Seq = rc.SEQ.Value,
                                                CheckItemList = (from x in db.ROUTECONTROLPOINTCHECKITEM
                                                                 join c in db.CHECKITEM
                                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                                 where x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID
                                                                 select new CheckItemModel
                                                                 {
                                                                     IsChecked = true,
                                                                     UniqueID = c.UNIQUEID,
                                                                     CheckType = c.CHECKTYPE,
                                                                     CheckItemID = c.ID,
                                                                     CheckItemDescription = c.DESCRIPTION,
                                                                     Seq = x.SEQ.Value
                                                                 }).OrderBy(x => x.Seq).ToList(),
                                            }).OrderBy(x => x.Seq).ToList()
                    };

                    foreach (var controlPoint in model.ControlPointList)
                    {
                        var equipmentList = (from re in db.ROUTEEQUIPMENT
                                             join equipment in db.EQUIPMENT
                                             on re.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                                             where re.ROUTEUNIQUEID == route.UNIQUEID && re.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                             select new EquipmentModel
                                             {
                                                 EquipmentUniqueID = equipment.UNIQUEID,
                                                 PartUniqueID = re.PARTUNIQUEID,
                                                 EquipmentID = equipment.ID,
                                                 EquipmentName = equipment.NAME,
                                                 Seq = re.SEQ.Value
                                             }).OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == equipment.PartUniqueID);

                            controlPoint.EquipmentList.Add(new EquipmentModel()
                            {
                                EquipmentUniqueID = equipment.EquipmentUniqueID,
                                PartUniqueID = part != null ? part.UNIQUEID : "*",
                                EquipmentID = equipment.EquipmentID,
                                EquipmentName = equipment.EquipmentName,
                                PartDescription = part != null ? part.DESCRIPTION : "",
                                Seq = equipment.Seq,
                                CheckItemList = (from x in db.ROUTEEQUIPMENTCHECKITEM
                                                 join c in db.CHECKITEM
                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                 where x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID
                                                 select new CheckItemModel
                                                 {
                                                     IsChecked = true,
                                                     UniqueID = c.UNIQUEID,
                                                     CheckType = c.CHECKTYPE,
                                                     CheckItemID = c.ID,
                                                     CheckItemDescription = c.DESCRIPTION,
                                                     Seq = x.SEQ.Value
                                                 }).OrderBy(x => x.Seq).ToList()
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var job = (from j in db.JOB
                               join r in db.ROUTE
                               on j.ROUTEUNIQUEID equals r.UNIQUEID
                               where j.UNIQUEID == UniqueID
                               select new { Job = j, Route = r }).First();

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.Route.ORGANIZATIONUNIQUEID),
                        RouteUniqueID = job.Route.UNIQUEID,
                        RouteID = job.Route.ID,
                        RouteName = job.Route.NAME,
                        FormInput = new FormInput()
                        {
                            IsCheckBySeq = job.Job.ISCHECKBYSEQ=="Y",
                            IsNeedVerify = job.Job.ISNEEDVERIFY == "Y",
                            IsShowPrevRecord = job.Job.ISSHOWPREVRECORD == "Y",
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.BEGINDATE),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.ENDDATE),
                            TimeMode = job.Job.TIMEMODE.Value,
                            BeginTime = job.Job.BEGINTIME,
                            EndTime = job.Job.ENDTIME,
                            CycleCount = job.Job.CYCLECOUNT.Value,
                            CycleMode = job.Job.CYCLEMODE,
                            Remark = job.Job.REMARK
                        },
                        UserList = db.JOBUSER.Where(x => x.JOBUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.JobManagement.UserModel
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    var jobControlPointList = db.JOBCONTROLPOINT.Where(x => x.JOBUNIQUEID == UniqueID).ToList();
                    var jobControlPointCheckItemList = db.JOBCONTROLPOINTCHECKITEM.Where(x => x.JOBUNIQUEID == UniqueID).ToList();
                    var jobEquipmentList = db.JOBEQUIPMENT.Where(x => x.JOBUNIQUEID == UniqueID).ToList();
                    var jobEquipmentCheckItemList = db.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == UniqueID).ToList();

                    var routeControlPointList = (from x in db.ROUTECONTROLPOINT
                                                 join c in db.CONTROLPOINT
                                                 on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                                 where x.ROUTEUNIQUEID == model.RouteUniqueID
                                                 select new
                                                 {
                                                     c.UNIQUEID,
                                                     c.ID,
                                                     c.DESCRIPTION,
                                                     x.SEQ
                                                 }).ToList();

                    foreach (var routeControlPoint in routeControlPointList)
                    {
                        var jobControlPoint = jobControlPointList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID);

                        var controlPointModel = new ControlPointModel()
                        {
                            UniqueID = routeControlPoint.UNIQUEID,
                            ID = routeControlPoint.ID,
                            Description = routeControlPoint.DESCRIPTION,
                            MinTimeSpan = jobControlPoint != null ? jobControlPoint.MINTIMESPAN : default(int?),
                            Seq = routeControlPoint.SEQ.Value
                        };

                        var routeControlPointCheckItemList = (from x in db.ROUTECONTROLPOINTCHECKITEM
                                                              join c in db.CHECKITEM
                                                              on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                              where x.ROUTEUNIQUEID == model.RouteUniqueID && x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID
                                                              select new
                                                              {
                                                                  c.UNIQUEID,
                                                                  c.CHECKTYPE,
                                                                  c.ID,
                                                                  c.DESCRIPTION,
                                                                  x.SEQ
                                                              }).ToList();

                        foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                        {
                            var jobControlPointCheckItem = jobControlPointCheckItemList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.CHECKITEMUNIQUEID == routeControlPointCheckItem.UNIQUEID);

                            controlPointModel.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = jobControlPointCheckItem != null,
                                UniqueID = routeControlPointCheckItem.UNIQUEID,
                                CheckType = routeControlPointCheckItem.CHECKTYPE,
                                CheckItemID = routeControlPointCheckItem.ID,
                                CheckItemDescription = routeControlPointCheckItem.DESCRIPTION,
                                Seq = routeControlPointCheckItem.SEQ.Value
                            });
                        }

                        controlPointModel.CheckItemList = controlPointModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                        var routeEquipmntList = (from x in db.ROUTEEQUIPMENT
                                                 join e in db.EQUIPMENT
                                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                 join p in db.EQUIPMENTPART
                                                 on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                                 from p in tmpPart.DefaultIfEmpty()
                                                 where x.ROUTEUNIQUEID == model.RouteUniqueID && x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UNIQUEID,
                                                     PartUniqueID = p != null ? p.UNIQUEID : "*",
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.NAME,
                                                     PartDescription = p != null ? p.DESCRIPTION : "",
                                                     x.SEQ
                                                 }).ToList();

                        foreach (var routeEquipment in routeEquipmntList)
                        {
                            var jobEquipment = jobEquipmentList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EquipmentUniqueID && x.PARTUNIQUEID == routeEquipment.PartUniqueID);

                            var equipmentModel = new EquipmentModel()
                            {
                                EquipmentUniqueID = routeEquipment.EquipmentUniqueID,
                                PartUniqueID = routeEquipment.PartUniqueID,
                                EquipmentID = routeEquipment.EquipmentID,
                                EquipmentName = routeEquipment.EquipmentName,
                                PartDescription = routeEquipment.PartDescription,
                                Seq = routeEquipment.SEQ.Value
                            };

                            var routeEquipmentCheckItemList = (from x in db.ROUTEEQUIPMENTCHECKITEM
                                                               join c in db.CHECKITEM
                                                               on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                               where x.ROUTEUNIQUEID == model.RouteUniqueID && x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EquipmentUniqueID && x.PARTUNIQUEID == routeEquipment.PartUniqueID
                                                               select new
                                                               {
                                                                   c.UNIQUEID,
                                                                   c.CHECKTYPE,
                                                                   c.ID,
                                                                   c.DESCRIPTION,
                                                                   x.SEQ
                                                               }).ToList();

                            foreach (var routeEquipmentCheckItem in routeEquipmentCheckItemList)
                            {
                                var jobEquipmentCheckItem = jobEquipmentCheckItemList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EquipmentUniqueID && x.PARTUNIQUEID == routeEquipment.PartUniqueID && x.CHECKITEMUNIQUEID == routeEquipmentCheckItem.UNIQUEID);

                                equipmentModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = jobEquipmentCheckItem != null,
                                    UniqueID = routeEquipmentCheckItem.UNIQUEID,
                                    CheckType = routeEquipmentCheckItem.CHECKTYPE,
                                    CheckItemID = routeEquipmentCheckItem.ID,
                                    CheckItemDescription = routeEquipmentCheckItem.DESCRIPTION,
                                    Seq = routeEquipmentCheckItem.SEQ.Value
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.ACCOUNT
                                      on user.ID equals u.ID
                                      join o in db.ORGANIZATION
                                      on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                      select new Models.EquipmentMaintenance.JobManagement.UserModel
                                      {
                                          OrganizationDescription = o.DESCRIPTION,
                                          ID = u.ID,
                                          Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.JOB.FirstOrDefault(x => x.ROUTEUNIQUEID == Model.RouteUniqueID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.JOB.Add(new JOB()
                        {
                            UNIQUEID = uniqueID,
                            ROUTEUNIQUEID = Model.RouteUniqueID,
                            DESCRIPTION = Model.FormInput.Description,
                            TIMEMODE = Model.FormInput.TimeMode,
                            BEGINTIME = Model.FormInput.BeginTime,
                            ENDTIME = Model.FormInput.EndTime,
                            ISCHECKBYSEQ = Model.FormInput.IsCheckBySeq?"Y":"N",
                            ISSHOWPREVRECORD = Model.FormInput.IsShowPrevRecord ? "Y" : "N",
                            ISNEEDVERIFY = Model.FormInput.IsNeedVerify ? "Y" : "N",
                            CYCLECOUNT = Model.FormInput.CycleCount,
                            CYCLEMODE = Model.FormInput.CycleMode,
                            BEGINDATE = Model.FormInput.BeginDate,
                            ENDDATE = Model.FormInput.EndDate,
                            REMARK = Model.FormInput.Remark,
                            LASTMODIFYTIME = DateTime.Now
                        });

                        db.JOBUSER.AddRange(Model.UserList.Select(x => new JOBUSER
                        {
                            JOBUNIQUEID = uniqueID,
                            USERID = x.ID
                        })).ToList();

                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            if (controlPoint.CheckItemList.Count > 0 || controlPoint.EquipmentList.Any(x => x.CheckItemList.Count(c => c.IsChecked) > 0))
                            {
                                db.JOBCONTROLPOINT.Add(new JOBCONTROLPOINT()
                                {
                                    JOBUNIQUEID = uniqueID,
                                    CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                    MINTIMESPAN = controlPoint.MinTimeSpan
                                });

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.JOBCONTROLPOINTCHECKITEM.Add(new JOBCONTROLPOINTCHECKITEM()
                                        {
                                            JOBUNIQUEID = uniqueID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            CHECKITEMUNIQUEID = checkItem.UniqueID
                                        });
                                    }
                                }

                                foreach (var equipment in controlPoint.EquipmentList)
                                {
                                    if (equipment.CheckItemList.Count(x => x.IsChecked) > 0)
                                    {
                                        db.JOBEQUIPMENT.Add(new JOBEQUIPMENT()
                                        {
                                            JOBUNIQUEID = uniqueID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                            PARTUNIQUEID = equipment.PartUniqueID
                                        });

                                        foreach (var checkItem in equipment.CheckItemList)
                                        {
                                            if (checkItem.IsChecked)
                                            {
                                                db.JOBEQUIPMENTCHECKITEM.Add(new JOBEQUIPMENTCHECKITEM()
                                                {
                                                    JOBUNIQUEID = uniqueID,
                                                    CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                                    EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                                    PARTUNIQUEID = equipment.PartUniqueID,
                                                    CHECKITEMUNIQUEID = checkItem.UniqueID
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var job = (from j in db.JOB
                               join r in db.ROUTE
                               on j.ROUTEUNIQUEID equals r.UNIQUEID
                               where j.UNIQUEID == UniqueID
                               select new { Job = j, Route = r }).First();

                    model = new EditFormModel()
                    {
                        UniqueID = job.Job.UNIQUEID,
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.Route.ORGANIZATIONUNIQUEID),
                        RouteUniqueID = job.Route.UNIQUEID,
                        RouteID = job.Route.ID,
                        RouteName = job.Route.NAME,
                        FormInput = new FormInput()
                        {
                            Description = job.Job.DESCRIPTION,
                            IsCheckBySeq = job.Job.ISCHECKBYSEQ=="Y",
                            IsNeedVerify = job.Job.ISNEEDVERIFY == "Y",
                            IsShowPrevRecord = job.Job.ISSHOWPREVRECORD == "Y",
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.BEGINDATE),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.Job.ENDDATE),
                            TimeMode = job.Job.TIMEMODE.Value,
                            BeginTime = job.Job.BEGINTIME,
                            EndTime = job.Job.ENDTIME,
                            CycleCount = job.Job.CYCLECOUNT.Value,
                            CycleMode = job.Job.CYCLEMODE,
                            Remark = job.Job.REMARK
                        },
                        UserList = db.JOBUSER.Where(x => x.JOBUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.JobManagement.UserModel
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    var jobControlPointList = db.JOBCONTROLPOINT.Where(x => x.JOBUNIQUEID == UniqueID).ToList();
                    var jobControlPointCheckItemList = db.JOBCONTROLPOINTCHECKITEM.Where(x => x.JOBUNIQUEID == UniqueID).ToList();
                    var jobEquipmentList = db.JOBEQUIPMENT.Where(x => x.JOBUNIQUEID == UniqueID).ToList();
                    var jobEquipmentCheckItemList = db.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == UniqueID).ToList();

                    var routeControlPointList = (from x in db.ROUTECONTROLPOINT
                                                 join c in db.CONTROLPOINT
                                                 on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                                 where x.ROUTEUNIQUEID == model.RouteUniqueID
                                                 select new
                                                 {
                                                     c.UNIQUEID,
                                                     c.ID,
                                                     c.DESCRIPTION,
                                                     x.SEQ
                                                 }).ToList();

                    foreach (var routeControlPoint in routeControlPointList)
                    {
                        var jobControlPoint = jobControlPointList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID);

                        var controlPointModel = new ControlPointModel()
                        {
                            UniqueID = routeControlPoint.UNIQUEID,
                            ID = routeControlPoint.ID,
                            Description = routeControlPoint.DESCRIPTION,
                            MinTimeSpan = jobControlPoint != null ? jobControlPoint.MINTIMESPAN : default(int?),
                            Seq = routeControlPoint.SEQ.Value
                        };

                        var routeControlPointCheckItemList = (from x in db.ROUTECONTROLPOINTCHECKITEM
                                                              join c in db.CHECKITEM
                                                              on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                              where x.ROUTEUNIQUEID == model.RouteUniqueID && x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID
                                                              select new
                                                              {
                                                                  c.UNIQUEID,
                                                                  c.CHECKTYPE,
                                                                  c.ID,
                                                                  c.DESCRIPTION,
                                                                  x.SEQ
                                                              }).ToList();

                        foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                        {
                            var jobControlPointCheckItem = jobControlPointCheckItemList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.CHECKITEMUNIQUEID == routeControlPointCheckItem.UNIQUEID);

                            controlPointModel.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = jobControlPointCheckItem != null,
                                UniqueID = routeControlPointCheckItem.UNIQUEID,
                                CheckType = routeControlPointCheckItem.CHECKTYPE,
                                CheckItemID = routeControlPointCheckItem.ID,
                                CheckItemDescription = routeControlPointCheckItem.DESCRIPTION,
                                Seq = routeControlPointCheckItem.SEQ.Value
                            });
                        }

                        controlPointModel.CheckItemList = controlPointModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                        var routeEquipmntList = (from x in db.ROUTEEQUIPMENT
                                                 join e in db.EQUIPMENT
                                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                 join p in db.EQUIPMENTPART
                                                 on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                                 from p in tmpPart.DefaultIfEmpty()
                                                 where x.ROUTEUNIQUEID == model.RouteUniqueID && x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UNIQUEID,
                                                     PartUniqueID = p != null ? p.UNIQUEID : "*",
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.NAME,
                                                     PartDescription = p != null ? p.DESCRIPTION : "",
                                                     x.SEQ
                                                 }).ToList();

                        foreach (var routeEquipment in routeEquipmntList)
                        {
                            var jobEquipment = jobEquipmentList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EquipmentUniqueID && x.PARTUNIQUEID == routeEquipment.PartUniqueID);

                            var equipmentModel = new EquipmentModel()
                            {
                                EquipmentUniqueID = routeEquipment.EquipmentUniqueID,
                                PartUniqueID = routeEquipment.PartUniqueID,
                                EquipmentID = routeEquipment.EquipmentID,
                                EquipmentName = routeEquipment.EquipmentName,
                                PartDescription = routeEquipment.PartDescription,
                                Seq = routeEquipment.SEQ.Value
                            };

                            var routeEquipmentCheckItemList = (from x in db.ROUTEEQUIPMENTCHECKITEM
                                                               join c in db.CHECKITEM
                                                               on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                               where x.ROUTEUNIQUEID == model.RouteUniqueID && x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EquipmentUniqueID && x.PARTUNIQUEID == routeEquipment.PartUniqueID
                                                               select new
                                                               {
                                                                   c.UNIQUEID,
                                                                   c.CHECKTYPE,
                                                                   c.ID,
                                                                   c.DESCRIPTION,
                                                                   x.SEQ
                                                               }).ToList();

                            foreach (var routeEquipmentCheckItem in routeEquipmentCheckItemList)
                            {
                                var jobEquipmentCheckItem = jobEquipmentCheckItemList.FirstOrDefault(x => x.CONTROLPOINTUNIQUEID == routeControlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EquipmentUniqueID && x.PARTUNIQUEID == routeEquipment.PartUniqueID && x.CHECKITEMUNIQUEID == routeEquipmentCheckItem.UNIQUEID);

                                equipmentModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = jobEquipmentCheckItem != null,
                                    UniqueID = routeEquipmentCheckItem.UNIQUEID,
                                    CheckType = routeEquipmentCheckItem.CHECKTYPE,
                                    CheckItemID = routeEquipmentCheckItem.ID,
                                    CheckItemDescription = routeEquipmentCheckItem.DESCRIPTION,
                                    Seq = routeEquipmentCheckItem.SEQ.Value
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.ACCOUNT
                                      on user.ID equals u.ID
                                      join o in db.ORGANIZATION
                                      on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                      select new Models.EquipmentMaintenance.JobManagement.UserModel
                                      {
                                          OrganizationDescription = o.DESCRIPTION,
                                          ID = u.ID,
                                          Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var job = db.JOB.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.JOB.FirstOrDefault(x => x.UNIQUEID != job.UNIQUEID && x.ROUTEUNIQUEID == job.ROUTEUNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Job
                        job.DESCRIPTION = Model.FormInput.Description;
                        job.ISCHECKBYSEQ = Model.FormInput.IsCheckBySeq?"Y":"N";
                        job.ISNEEDVERIFY = Model.FormInput.IsNeedVerify ? "Y" : "N";
                        job.ISSHOWPREVRECORD = Model.FormInput.IsShowPrevRecord ? "Y" : "N";
                        job.BEGINDATE = Model.FormInput.BeginDate;
                        job.ENDDATE = Model.FormInput.EndDate;
                        job.TIMEMODE = Model.FormInput.TimeMode;
                        job.BEGINTIME = Model.FormInput.BeginTime;
                        job.ENDTIME = Model.FormInput.EndTime;
                        job.CYCLECOUNT = Model.FormInput.CycleCount;
                        job.CYCLEMODE = Model.FormInput.CycleMode;
                        job.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region JobUser
                        #region Delete
                        db.JOBUSER.RemoveRange(db.JOBUSER.Where(x => x.JOBUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.JOBUSER.AddRange(Model.UserList.Select(x => new JOBUSER
                        {
                            JOBUNIQUEID      = job.UNIQUEID,
                            USERID = x.ID
                        })).ToList();
                        #endregion
                        #endregion

                        #region JobControlPoint, JobControlPointCheckItem, JobEquipment, JobEquipmentCheckItem
                        #region Delete
                        db.JOBCONTROLPOINT.RemoveRange(db.JOBCONTROLPOINT.Where(x => x.JOBUNIQUEID == job.UNIQUEID).ToList());
                        db.JOBCONTROLPOINTCHECKITEM.RemoveRange(db.JOBCONTROLPOINTCHECKITEM.Where(x => x.JOBUNIQUEID == job.UNIQUEID).ToList());
                        db.JOBEQUIPMENT.RemoveRange(db.JOBEQUIPMENT.Where(x => x.JOBUNIQUEID == job.UNIQUEID).ToList());
                        db.JOBEQUIPMENTCHECKITEM.RemoveRange(db.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == job.UNIQUEID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            if (controlPoint.CheckItemList.Count > 0 || controlPoint.EquipmentList.Any(x => x.CheckItemList.Count(c => c.IsChecked) > 0))
                            {
                                db.JOBCONTROLPOINT.Add(new JOBCONTROLPOINT()
                                {
                                    JOBUNIQUEID = job.UNIQUEID,
                                    CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                    MINTIMESPAN = controlPoint.MinTimeSpan
                                });

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.JOBCONTROLPOINTCHECKITEM.Add(new JOBCONTROLPOINTCHECKITEM()
                                        {
                                            JOBUNIQUEID = job.UNIQUEID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            CHECKITEMUNIQUEID = checkItem.UniqueID
                                        });
                                    }
                                }

                                foreach (var equipment in controlPoint.EquipmentList)
                                {
                                    if (equipment.CheckItemList.Count(x => x.IsChecked) > 0)
                                    {
                                        db.JOBEQUIPMENT.Add(new JOBEQUIPMENT()
                                        {
                                            JOBUNIQUEID = job.UNIQUEID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                            PARTUNIQUEID = equipment.PartUniqueID
                                        });

                                        foreach (var checkItem in equipment.CheckItemList)
                                        {
                                            if (checkItem.IsChecked)
                                            {
                                                db.JOBEQUIPMENTCHECKITEM.Add(new JOBEQUIPMENTCHECKITEM()
                                                {
                                                    JOBUNIQUEID = job.UNIQUEID,
                                                    CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                                    EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                                    PARTUNIQUEID = equipment.PartUniqueID,
                                                    CHECKITEMUNIQUEID = checkItem.UniqueID
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

                        DateTime beginDate = DateTime.Today;

                        if (JobCycleHelper.IsInCycle(DateTime.Today, Model.FormInput.BeginDate, Model.FormInput.EndDate, Model.FormInput.CycleCount, Model.FormInput.CycleMode))
                        {
                            DateTime endDate;

                            JobCycleHelper.GetDateSpan(DateTime.Today, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out beginDate, out endDate);
                        }

                        var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);

                        var jobResultList = db.JOBRESULT.Where(x => x.JOBUNIQUEID == Model.UniqueID && string.Compare(x.BEGINDATE, beginDateString) >= 0).ToList();

                        foreach (var jobResult in jobResultList)
                        {
                            JobResultDataAccessor.Refresh(jobResult.UNIQUEID, jobResult.JOBUNIQUEID, jobResult.BEGINDATE, jobResult.ENDDATE);
                        }
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPointList = (from x in db.JOBCONTROLPOINT
                                            join j in db.JOB
                                            on x.JOBUNIQUEID equals j.UNIQUEID
                                            join y in db.ROUTECONTROLPOINT
                                            on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID }
                                            join c in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                            where x.JOBUNIQUEID == JobUniqueID
                                            select new
                                            {
                                                c.UNIQUEID,
                                                c.ID,
                                                c.DESCRIPTION,
                                                y.SEQ
                                            }).OrderBy(x => x.SEQ).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.DESCRIPTION };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.DESCRIPTION);
                        attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UNIQUEID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.JOBEQUIPMENT.Any(x => x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID)
                            ||
                            db.JOBCONTROLPOINTCHECKITEM.Any(x => x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID))
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(PartUniqueID) && PartUniqueID!="*")
                    {
                        var checkItemList = (from x in db.JOBEQUIPMENTCHECKITEM
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTEEQUIPMENTCHECKITEM
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID
                                             select new
                                             {
                                                 c.ID,
                                                 c.DESCRIPTION,
                                                 y.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
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
                        var checkItemList = (from x in db.JOBEQUIPMENTCHECKITEM
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTEEQUIPMENTCHECKITEM
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == "*"
                                             select new
                                             {
                                                 c.ID,
                                                 c.DESCRIPTION,
                                                 y.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
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
                        var checkItemList = (from x in db.JOBCONTROLPOINTCHECKITEM
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTECONTROLPOINTCHECKITEM
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.CHECKITEMUNIQUEID }
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID
                                             select new
                                             {
                                                 c.ID,
                                                 c.DESCRIPTION,
                                                 y.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();


                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
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

                        var equipmentList = (from x in db.JOBEQUIPMENT
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTEEQUIPMENT
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                             join e in db.EQUIPMENT
                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                             join p in db.EQUIPMENTPART
                                             on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             where x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID
                                             select new
                                             {
                                                 EquipmentUniqueID = e.UNIQUEID,
                                                 PartUniqueID = p != null ? p.UNIQUEID : "*",
                                                 EquipmentID = e.ID,
                                                 EquipmentName = e.NAME,
                                                 PartDescription = p != null ? p.DESCRIPTION : "",
                                                 y.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();

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

                            if (db.JOBEQUIPMENTCHECKITEM.Any(x => x.JOBUNIQUEID == JobUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID))
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

                using (ASEDbEntities db = new ASEDbEntities())
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
                                UserList.Add((from u in db.ACCOUNT
                                              join o in db.ORGANIZATION
                                                  on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                              where u.ID == userID
                                              select new Models.EquipmentMaintenance.JobManagement.UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.NAME,
                                                  OrganizationDescription = o.DESCRIPTION
                                              }).First());
                            }
                        }
                        else
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);
                            var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                            var userList = (from u in db.ACCOUNT
                                            join o in db.ORGANIZATION
                                           on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                            where organizationList.Contains(u.ORGANIZATIONUNIQUEID)
                                            select new
                                            {
                                                ID = u.ID,
                                                Name = u.NAME,
                                                OrganizationDescription = o.DESCRIPTION
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
