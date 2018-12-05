using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.EquipmentMaintenance.MaintenanceJobManagement;
using Models.Authenticated;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class MaintenanceJobDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.MJOB.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UNIQUEID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            Description = x.DESCRIPTION,
                            CycleCount = x.CYCLECOUNT.Value,
                            CycleMode = x.CYCLEMODE
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.Description).ToList()
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
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var j = db.MJOB.First(x => x.UNIQUEID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = j.UNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(j.ORGANIZATIONUNIQUEID),
                        Description = j.DESCRIPTION,
                        NotifyDay = j.NOTIFYDAY.Value,
                        BeginDate = j.BEGINDATE.Value,
                        EndDate = j.ENDDATE,
                        CycleCount = j.CYCLECOUNT.Value,
                        CycleMode = j.CYCLEMODE,
                        Remark = j.REMARK,
                        UserList = db.MJOBUSER.Where(x => x.MJOBUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                        {
                            ID = x.USERID
                        }).ToList()
                    };


                    model.UserList = (from user in model.UserList
                                      join u in db.ACCOUNT
                                      on user.ID equals u.ID
                                      join o in db.ORGANIZATION
                                      on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                      select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var job = db.MJOB.First(x => x.UNIQUEID == UniqueID);

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = job.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            NotifyDay = job.NOTIFYDAY.Value,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.BEGINDATE),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.ENDDATE),
                            CycleCount = job.CYCLECOUNT.Value,
                            CycleMode = job.CYCLEMODE,
                            Remark = job.REMARK
                        },
                        UserList = db.MJOBUSER.Where(x => x.MJOBUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    var equipmentList = (from x in db.MJOBEQUIPMENT
                                         join e in db.EQUIPMENT
                                         on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                         where x.MJOBUNIQUEID == job.UNIQUEID
                                         select new
                                         {
                                             x = x,
                                             e = e
                                         }).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == equipment.x.PARTUNIQUEID);

                        var equipmentModel = new EquipmentModel()
                        {
                            EquipmentUniqueID = equipment.x.EQUIPMENTUNIQUEID,
                            PartUniqueID = equipment.x.PARTUNIQUEID,
                            EquipmentID = equipment.e.ID,
                            EquipmentName = equipment.e.NAME,
                            PartDescription = part != null ? part.DESCRIPTION : string.Empty
                        };

                        var equipmentStandardList = (from x in db.EQUIPMENTSTANDARD
                                                     join s in db.STANDARD
                                                     on x.STANDARDUNIQUEID equals s.UNIQUEID
                                                     where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         s.UNIQUEID,
                                                         MaintenanceType = s.MAINTENANCETYPE,
                                                         s.ID,
                                                         s.DESCRIPTION
                                                     }).ToList();

                        foreach (var standard in equipmentStandardList)
                        {
                            var jobEquipmentStandard = db.MJOBEQUIPMENTSTANDARD.FirstOrDefault(x => x.MJOBUNIQUEID == job.UNIQUEID && x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID && x.STANDARDUNIQUEID == standard.UNIQUEID);

                            equipmentModel.StandardList.Add(new StandardModel()
                            {
                                IsChecked = jobEquipmentStandard != null,
                                UniqueID = standard.UNIQUEID,
                                MaintenanceType = standard.MaintenanceType,
                                StandardID = standard.ID,
                                StandardDescription = standard.DESCRIPTION
                            });
                        }

                        var equipmentMaterialList = (from x in db.EQUIPMENTMATERIAL
                                                     join m in db.MATERIAL
                                                     on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                     where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         m.UNIQUEID,
                                                         m.ID,
                                                         m.NAME,
                                                        Quantity= x.QUANTITY.Value
                                                     }).ToList();

                        foreach (var material in equipmentMaterialList)
                        {
                            var jobEquipmentMaterial = db.MJOBEQUIPMENTMATERIAL.FirstOrDefault(x => x.MJOBUNIQUEID == job.UNIQUEID && x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID && x.MATERIALUNIQUEID == material.UNIQUEID);

                            equipmentModel.MaterialList.Add(new MaterialModel()
                            {
                                IsChecked = jobEquipmentMaterial != null,
                                UniqueID = material.UNIQUEID,
                                ID = material.ID,
                                Name = material.NAME,
                                Quantity = material.Quantity
                            });
                        }

                        equipmentModel.StandardList = equipmentModel.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();
                        equipmentModel.MaterialList = equipmentModel.MaterialList.OrderBy(x => x.ID).ToList();

                        model.EquipmentList.Add(equipmentModel);
                    }

                    model.EquipmentList = model.EquipmentList.OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.ACCOUNT
                                      on user.ID equals u.ID
                                      join o in db.ORGANIZATION
                                      on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                      select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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
                    var exists = db.MJOB.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        if (DateTime.Compare(Model.FormInput.BeginDate, DateTime.Today) > 0)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            var mJob = new MJOB()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                DESCRIPTION = Model.FormInput.Description,
                                NOTIFYDAY = Model.FormInput.NotifyDay,
                                CYCLECOUNT = Model.FormInput.CycleCount,
                                CYCLEMODE = Model.FormInput.CycleMode,
                                BEGINDATE = Model.FormInput.BeginDate,
                                ENDDATE = Model.FormInput.EndDate,
                                REMARK = Model.FormInput.Remark,
                                LASTMODIFYTIME = DateTime.Now
                            };

                            //if (Model.FormInput.CycleMode == "D")
                            //{
                            //    mJob.DAYMODE = Model.FormInput.DayMode;
                            //}

                            //if (Model.FormInput.CycleMode == "W")
                            //{
                            //    mJob.WEEKMODE = Model.FormInput.WeekMode;

                            //    if (Model.FormInput.WeekMode == "S")
                            //    {
                            //        mJob.MON = Model.FormInput.Mon ? "Y" : "N";
                            //        mJob.TUE = Model.FormInput.Tue ? "Y" : "N";
                            //        mJob.WED = Model.FormInput.Wed ? "Y" : "N";
                            //        mJob.THU = Model.FormInput.Thu ? "Y" : "N";
                            //        mJob.FRI = Model.FormInput.Fri ? "Y" : "N";
                            //        mJob.SAT = Model.FormInput.Sat ? "Y" : "N";
                            //        mJob.SUN = Model.FormInput.Sun ? "Y" : "N";
                            //    }
                            //}

                            //if (Model.FormInput.CycleMode == "M")
                            //{
                            //    mJob.MONTHMODE = Model.FormInput.MonthMode;

                            //    if (Model.FormInput.MonthMode == "S")
                            //    {
                            //        mJob.DAY = Model.FormInput.Day;
                            //    }
                            //}

                            db.MJOB.Add(mJob);

                            db.MJOBUSER.AddRange(Model.UserList.Select(x => new MJOBUSER
                            {
                                MJOBUNIQUEID = uniqueID,
                                USERID = x.ID
                            })).ToList();

                            foreach (var equipment in Model.EquipmentList)
                            {
                                if (equipment.StandardList.Count(x => x.IsChecked) > 0 || equipment.MaterialList.Count(x => x.IsChecked && x.ChangeQuantity.HasValue) > 0)
                                {
                                    db.MJOBEQUIPMENT.Add(new MJOBEQUIPMENT()
                                    {
                                        MJOBUNIQUEID = uniqueID,
                                        EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                        PARTUNIQUEID = equipment.PartUniqueID
                                    });

                                    foreach (var standard in equipment.StandardList)
                                    {
                                        if (standard.IsChecked)
                                        {
                                            db.MJOBEQUIPMENTSTANDARD.Add(new MJOBEQUIPMENTSTANDARD()
                                            {
                                                MJOBUNIQUEID = uniqueID,
                                                EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                                PARTUNIQUEID = equipment.PartUniqueID,
                                                STANDARDUNIQUEID = standard.UniqueID
                                            });
                                        }
                                    }

                                    foreach (var material in equipment.MaterialList)
                                    {
                                        if (material.IsChecked && material.ChangeQuantity.HasValue)
                                        {
                                            db.MJOBEQUIPMENTMATERIAL.Add(new MJOBEQUIPMENTMATERIAL()
                                            {
                                                MJOBUNIQUEID = uniqueID,
                                                EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                                PARTUNIQUEID = equipment.PartUniqueID,
                                                MATERIALUNIQUEID = material.UniqueID,
                                                QUANTITY = material.ChangeQuantity.Value
                                            });
                                        }
                                    }
                                }
                            }

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.MaintenanceJob, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(Resources.Resource.JobBeginDateHave2AfterToday);
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.CycleMode, Resources.Resource.Exists));
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
                    var job = db.MJOB.First(x => x.UNIQUEID == UniqueID);

                    model = new EditFormModel()
                    {
                        UniqueID = job.UNIQUEID,
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = job.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            Description = job.DESCRIPTION,
                            NotifyDay = job.NOTIFYDAY.Value,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.BEGINDATE),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.ENDDATE),
                            CycleCount = job.CYCLECOUNT.Value,
                            CycleMode = job.CYCLEMODE,
                            Remark = job.REMARK,
                            //Day = job.DAY,
                            //DayMode = job.DAYMODE,
                            //WeekMode = job.WEEKMODE,
                            //Mon = job.MON == "Y",
                            //Tue = job.TUE == "Y",
                            //Wed = job.WED == "Y",
                            //Thu = job.THU == "Y",
                            //Fri = job.FRI == "Y",
                            //Sat = job.SAT == "Y",
                            //Sun = job.SUN == "Y",
                            //MonthMode = job.MONTHMODE
                        },
                        UserList = db.MJOBUSER.Where(x => x.MJOBUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    var equipmentList = (from x in db.MJOBEQUIPMENT
                                         join e in db.EQUIPMENT
                                         on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                         where x.MJOBUNIQUEID == job.UNIQUEID
                                         select new
                                         {
                                             x = x,
                                             e = e
                                         }).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == equipment.x.PARTUNIQUEID);

                        var equipmentModel = new EquipmentModel()
                        {
                            EquipmentUniqueID = equipment.x.EQUIPMENTUNIQUEID,
                            PartUniqueID = equipment.x.PARTUNIQUEID,
                            EquipmentID = equipment.e.ID,
                            EquipmentName = equipment.e.NAME,
                            PartDescription = part != null ? part.DESCRIPTION : string.Empty
                        };

                        var equipmentStandardList = (from x in db.EQUIPMENTSTANDARD
                                                     join s in db.STANDARD
                                                     on x.STANDARDUNIQUEID equals s.UNIQUEID
                                                     where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         s.UNIQUEID,
                                                         MaintenanceType = s.MAINTENANCETYPE,
                                                         s.ID,
                                                         s.DESCRIPTION
                                                     }).ToList();

                        foreach (var standard in equipmentStandardList)
                        {
                            var jobEquipmentStandard = db.MJOBEQUIPMENTSTANDARD.FirstOrDefault(x => x.MJOBUNIQUEID == job.UNIQUEID && x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID && x.STANDARDUNIQUEID == standard.UNIQUEID);

                            equipmentModel.StandardList.Add(new StandardModel()
                            {
                                IsChecked = jobEquipmentStandard != null,
                                UniqueID = standard.UNIQUEID,
                                MaintenanceType = standard.MaintenanceType,
                                StandardID = standard.ID,
                                StandardDescription = standard.DESCRIPTION,
                                OptionList = db.STANDARDFEELOPTION.Where(o => o.STANDARDUNIQUEID == standard.UNIQUEID).Select(o => new FeelOptionModel
                                {
                                    UniqueID = o.UNIQUEID,
                                    Seq = o.SEQ.Value,
                                    Description = o.DESCRIPTION,
                                    IsAbnormal = o.ISABNORMAL=="Y"
                                }).OrderBy(o => o.Seq).ToList()
                            });
                        }

                        var equipmentMaterialList = (from x in db.EQUIPMENTMATERIAL
                                                     join m in db.MATERIAL
                                                     on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                     where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         m.UNIQUEID,
                                                         m.ID,
                                                         m.NAME,
                                                         Quantity = x.QUANTITY.Value
                                                     }).ToList();

                        foreach (var material in equipmentMaterialList)
                        {
                            var jobEquipmentMaterial = db.MJOBEQUIPMENTMATERIAL.FirstOrDefault(x => x.MJOBUNIQUEID == job.UNIQUEID && x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID && x.MATERIALUNIQUEID == material.UNIQUEID);

                            equipmentModel.MaterialList.Add(new MaterialModel()
                            {
                                IsChecked = jobEquipmentMaterial != null,
                                UniqueID = material.UNIQUEID,
                                ID = material.ID,
                                Name = material.NAME,
                                Quantity = material.Quantity
                            });
                        }

                        equipmentModel.StandardList = equipmentModel.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();
                        equipmentModel.MaterialList = equipmentModel.MaterialList.OrderBy(x => x.ID).ToList();

                        model.EquipmentList.Add(equipmentModel);
                    }

                    model.EquipmentList = model.EquipmentList.OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.ACCOUNT
                                      on user.ID equals u.ID
                                      join o in db.ORGANIZATION
                                      on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                      select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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
                    var job = db.MJOB.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.MJOB.FirstOrDefault(x => x.UNIQUEID != job.UNIQUEID && x.ORGANIZATIONUNIQUEID == job.ORGANIZATIONUNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        if (DateTime.Compare(Model.FormInput.BeginDate, DateTime.Today) > 0)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region MJob
                            job.    DESCRIPTION = Model.FormInput.Description;
job.                                NOTIFYDAY = Model.FormInput.NotifyDay;
                                job.CYCLECOUNT = Model.FormInput.CycleCount;
                                job.CYCLEMODE = Model.FormInput.CycleMode;
                                job.BEGINDATE = Model.FormInput.BeginDate;
                                job.ENDDATE = Model.FormInput.EndDate;
                                job.REMARK = Model.FormInput.Remark;
                                job.LASTMODIFYTIME = DateTime.Now;

                                //if (Model.FormInput.CycleMode == "D")
                                //{
                                //    job.DAYMODE = Model.FormInput.DayMode;
                                //}
                                //else
                                //{
                                //    job.DAYMODE = null;
                                //}

                                //if (Model.FormInput.CycleMode == "W")
                                //{
                                //    job.WEEKMODE = Model.FormInput.WeekMode;

                                //    if (Model.FormInput.WeekMode == "S")
                                //    {
                                //        job.MON = Model.FormInput.Mon ? "Y" : "N";
                                //        job.TUE = Model.FormInput.Tue ? "Y" : "N";
                                //        job.WED = Model.FormInput.Wed ? "Y" : "N";
                                //        job.THU = Model.FormInput.Thu ? "Y" : "N";
                                //        job.FRI = Model.FormInput.Fri ? "Y" : "N";
                                //        job.SAT = Model.FormInput.Sat ? "Y" : "N";
                                //        job.SUN = Model.FormInput.Sun ? "Y" : "N";
                                //    }
                                //    else
                                //    {
                                //        job.MON = null;
                                //        job.TUE = null;
                                //        job.WED = null;
                                //        job.THU = null;
                                //        job.FRI = null;
                                //        job.SAT = null;
                                //        job.SUN = null;
                                //    }
                                //}
                                //else
                                //{
                                //    job.WEEKMODE = null;
                                //    job.MON = null;
                                //    job.TUE = null;
                                //    job.WED = null;
                                //    job.THU = null;
                                //    job.FRI = null;
                                //    job.SAT = null;
                                //    job.SUN = null;
                                //}

                                //if (Model.FormInput.CycleMode == "M")
                                //{
                                //    job.MONTHMODE = Model.FormInput.MonthMode;

                                //    if (Model.FormInput.MonthMode == "S")
                                //    {
                                //        job.DAY = Model.FormInput.Day;
                                //    }
                                //    else
                                //    {
                                //        job.DAY = null;
                                //    }
                                //}
                                //else
                                //{
                                //    job.MONTHMODE = null;
                                //    job.DAY = null;
                                //}

                            db.SaveChanges();
                            #endregion

                            #region MJobUser
                            #region Delete
                            db.MJOBUSER.RemoveRange(db.MJOBUSER.Where(x => x.MJOBUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.MJOBUSER.AddRange(Model.UserList.Select(x => new MJOBUSER
                            {
                                MJOBUNIQUEID = job.UNIQUEID,
                                USERID = x.ID
                            })).ToList();
                            #endregion
                            #endregion

                            #region MJobEquipment, MJobEquipmentStandard
                            #region Delete
                            db.MJOBEQUIPMENT.RemoveRange(db.MJOBEQUIPMENT.Where(x => x.MJOBUNIQUEID == job.UNIQUEID).ToList());
                            db.MJOBEQUIPMENTSTANDARD.RemoveRange(db.MJOBEQUIPMENTSTANDARD.Where(x => x.MJOBUNIQUEID == job.UNIQUEID).ToList());
                            db.MJOBEQUIPMENTMATERIAL.RemoveRange(db.MJOBEQUIPMENTMATERIAL.Where(x => x.MJOBUNIQUEID == job.UNIQUEID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            foreach (var equipment in Model.EquipmentList)
                            {
                                if (equipment.StandardList.Count(x => x.IsChecked) > 0 || equipment.MaterialList.Count(x => x.IsChecked && x.ChangeQuantity.HasValue) > 0)
                                {
                                    db.MJOBEQUIPMENT.Add(new MJOBEQUIPMENT()
                                    {
                                        MJOBUNIQUEID = job.UNIQUEID,
                                        EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                        PARTUNIQUEID = equipment.PartUniqueID
                                    });

                                    foreach (var standard in equipment.StandardList)
                                    {
                                        if (standard.IsChecked)
                                        {
                                            db.MJOBEQUIPMENTSTANDARD.Add(new MJOBEQUIPMENTSTANDARD()
                                            {
                                                MJOBUNIQUEID = job.UNIQUEID,
                                                EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                                PARTUNIQUEID = equipment.PartUniqueID,
                                                STANDARDUNIQUEID = standard.UniqueID
                                            });
                                        }
                                    }

                                    foreach (var material in equipment.MaterialList)
                                    {
                                        if (material.IsChecked && material.ChangeQuantity.HasValue)
                                        {
                                            db.MJOBEQUIPMENTMATERIAL.Add(new MJOBEQUIPMENTMATERIAL()
                                            {
                                                MJOBUNIQUEID = job.UNIQUEID,
                                                EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                                PARTUNIQUEID = equipment.PartUniqueID,
                                                MATERIALUNIQUEID = material.UniqueID,
                                                QUANTITY = material.ChangeQuantity.Value
                                            });
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
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.MaintenanceJob, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(Resources.Resource.JobBeginDateHave2AfterToday);
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.CycleMode, Resources.Resource.Exists));
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

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        DeleteHelper.MaintenanceJob(db, uniqueID);
                    }
                    
                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.MaintenanceJob, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<EquipmentModel> EquipmentList, List<string> EquipmentStandardPageStateList, List<string> EquipmentMaterialPageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                
                foreach (var equipmentStandardPageState in EquipmentStandardPageStateList)
                {
                    string[] temp = equipmentStandardPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string equipmentUniqueID = temp[0];
                    string partUniqueID = temp[1];
                    string standardUniqueID = temp[2];
                    string isChecked = temp[3];

                    var standard = EquipmentList.First(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID).StandardList.First(x => x.UniqueID == standardUniqueID);

                    standard.IsChecked = isChecked == "Y";
                }

                foreach (var equipmentMaterialPageState in EquipmentMaterialPageStateList)
                {
                    string[] temp = equipmentMaterialPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string equipmentUniqueID = temp[0];
                    string partUniqueID = temp[1];
                    string materialUniqueID = temp[2];
                    string quantity = temp[3];
                    string isChecked = temp[4];

                    var material = EquipmentList.First(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID).MaterialList.First(x => x.UniqueID == materialUniqueID);

                    material.IsChecked = isChecked == "Y";

                    if (!string.IsNullOrEmpty(quantity))
                    {
                        material.ChangeQuantity = int.Parse(quantity);
                    }
                    else
                    {
                        material.ChangeQuantity = null;
                    }
                }

                foreach (var equipment in EquipmentList)
                {
                    equipment.StandardList = equipment.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x=>x.StandardID).ToList();
                    equipment.MaterialList = equipment.MaterialList.OrderBy(x => x.ID).ToList();
                }

                result.ReturnData(EquipmentList.OrderBy(x => x.EquipmentID).ThenBy(x=>x.PartDescription).ToList());
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipmentList = (from x in db.MJOBEQUIPMENT
                                         join e in db.EQUIPMENT
                                         on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                         join p in db.EQUIPMENTPART
                                         on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                         from p in tmpPart.DefaultIfEmpty()
                                         where x.MJOBUNIQUEID == JobUniqueID
                                         select new
                                         {
                                             EquipmentUniqueID = e.UNIQUEID,
                                             PartUniqueID = p != null ? p.UNIQUEID : "*",
                                             EquipmentID = e.ID,
                                             EquipmentName = e.NAME,
                                             PartDescription = p != null ? p.DESCRIPTION : ""
                                         }).OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var treeItem = new TreeItem();

                        if (equipment.PartUniqueID == "*")
                        {
                            treeItem.Title = equipment.EquipmentName;
                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.EquipmentID, equipment.EquipmentName);
                        }
                        else
                        {
                            treeItem.Title = string.Format("{0}-{1}", equipment.EquipmentName, equipment.PartDescription);
                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.EquipmentID, equipment.EquipmentName, equipment.PartDescription);
                        }

                        attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.EquipmentUniqueID;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = equipment.PartUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.MJOBEQUIPMENTSTANDARD.Any(x => x.MJOBUNIQUEID == JobUniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID) ||
                            db.MJOBEQUIPMENTMATERIAL.Any(x => x.MJOBUNIQUEID == JobUniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID))
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

        public static RequestResult GetDetailTreeItem(string JobUniqueID, string EquipmentUniqueID, string PartUniqueID)
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty },
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(PartUniqueID) && PartUniqueID != "*")
                    {
                        var standardList = (from x in db.MJOBEQUIPMENTSTANDARD
                                            join s in db.STANDARD
                                            on x.STANDARDUNIQUEID equals s.UNIQUEID
                                            where x.MJOBUNIQUEID == JobUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID
                                            select new
                                            {
                                                MAINTENANCETYPE = s.MAINTENANCETYPE,
                                                s.ID,
                                                s.DESCRIPTION
                                            }).OrderBy(x => x.MAINTENANCETYPE).ThenBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var materialList = (from x in db.MJOBEQUIPMENTMATERIAL
                                            join m in db.MATERIAL
                                            on x.MATERIALUNIQUEID equals m.UNIQUEID
                                            where x.MJOBUNIQUEID == JobUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID
                                            select new
                                            {
                                                m.ID,
                                                m.NAME,
                                                Quantity = x.QUANTITY
                                            }).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = string.Format("{0}({1})", material.NAME, material.Quantity) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var standardList = (from x in db.MJOBEQUIPMENTSTANDARD
                                            join s in db.STANDARD
                                            on x.STANDARDUNIQUEID equals s.UNIQUEID
                                            where x.MJOBUNIQUEID == JobUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == "*"
                                            select new
                                            {
                                                MAINTENANCETYPE = s.MAINTENANCETYPE,
                                                s.ID,
                                                s.DESCRIPTION
                                            }).OrderBy(x => x.MAINTENANCETYPE).ThenBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var materialList = (from x in db.MJOBEQUIPMENTMATERIAL
                                            join m in db.MATERIAL
                                            on x.MATERIALUNIQUEID equals m.UNIQUEID
                                            where x.MJOBUNIQUEID == JobUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == "*"
                                            select new
                                            {
                                                m.ID,
                                                m.NAME,
                                                Quantity = x.QUANTITY
                                            }).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = string.Format("{0}({1})", material.NAME, material.Quantity) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
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

        public static RequestResult AddEquipment(List<EquipmentModel> EquipmentList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var equipmentUniqueID = temp[1];
                        var partUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(partUniqueID) && partUniqueID != "*")
                        {
                            if (!EquipmentList.Any(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID))
                            {
                                var equipment = (from p in db.EQUIPMENTPART
                                                 join e in db.EQUIPMENT
                                                 on p.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                 where p.UNIQUEID == partUniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UNIQUEID,
                                                     PartUniqueID = p.UNIQUEID,
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.NAME,
                                                     PartDescription = p.DESCRIPTION
                                                 }).First();

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                    PartUniqueID = equipment.PartUniqueID,
                                    EquipmentID = equipment.EquipmentID,
                                    EquipmentName = equipment.EquipmentName,
                                    PartDescription = equipment.PartDescription
                                };

                                var standardList = (from x in db.EQUIPMENTSTANDARD
                                                    join s in db.STANDARD
                                                    on x.STANDARDUNIQUEID equals s.UNIQUEID
                                                    where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = s.UNIQUEID,
                                                        MaintenanceType = s.MAINTENANCETYPE,
                                                        StandardID = s.ID,
                                                        StandardDescription = s.DESCRIPTION
                                                    }).OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();

                                foreach (var standard in standardList)
                                {
                                    equipmentModel.StandardList.Add(new StandardModel()
                                    {
                                        IsChecked = true,
                                        UniqueID = standard.UniqueID,
                                        MaintenanceType = standard.MaintenanceType,
                                        StandardID = standard.StandardID,
                                        StandardDescription = standard.StandardDescription
                                    });
                                }

                                var materialList = (from x in db.EQUIPMENTMATERIAL
                                                    join m in db.MATERIAL
                                                    on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                    where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UNIQUEID,
                                                        ID = m.ID,
                                                        Name = m.NAME,
                                                        Quantity = x.QUANTITY.Value
                                                    }).OrderBy(x => x.ID).ToList();

                                foreach (var material in materialList)
                                {
                                    equipmentModel.MaterialList.Add(new MaterialModel() { 
                                     IsChecked=false,
                                      UniqueID = material.UniqueID,
                                       ID=material.ID,
                                        Name=material.Name,
                                         Quantity=material.Quantity
                                    });
                                }

                                EquipmentList.Add(equipmentModel);
                            }
                        }
                        else if (!string.IsNullOrEmpty(equipmentUniqueID))
                        {
                            if (!EquipmentList.Any(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == "*"))
                            {
                                var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == equipmentUniqueID);

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.UNIQUEID,
                                    PartUniqueID = "*",
                                    EquipmentID = equipment.ID,
                                    EquipmentName = equipment.NAME,
                                    PartDescription = "",
                                };

                                var standardList = (from x in db.EQUIPMENTSTANDARD
                                                    join s in db.STANDARD
                                                    on x.STANDARDUNIQUEID equals s.UNIQUEID
                                                    where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = s.UNIQUEID,
                                                        MaintenanceType = s.MAINTENANCETYPE,
                                                        StandardID = s.ID,
                                                        StandardDescription = s.DESCRIPTION
                                                    }).OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();

                                foreach (var standard in standardList)
                                {
                                    equipmentModel.StandardList.Add(new StandardModel()
                                    {
                                        IsChecked = true,
                                        UniqueID = standard.UniqueID,
                                        MaintenanceType = standard.MaintenanceType,
                                        StandardID = standard.StandardID,
                                        StandardDescription = standard.StandardDescription
                                    });
                                }

                                var materialList = (from x in db.EQUIPMENTMATERIAL
                                                    join m in db.MATERIAL
                                                    on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                    where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UNIQUEID,
                                                        ID = m.ID,
                                                        Name = m.NAME,
                                                        Quantity = x.QUANTITY.Value
                                                    }).OrderBy(x => x.ID).ToList();

                                foreach (var material in materialList)
                                {
                                    equipmentModel.MaterialList.Add(new MaterialModel()
                                    {
                                        IsChecked = false,
                                        UniqueID = material.UniqueID,
                                        ID = material.ID,
                                        Name = material.Name,
                                        Quantity = material.Quantity
                                    });
                                }

                                EquipmentList.Add(equipmentModel);
                            }
                        }
                        else
                        {
                            var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(RefOrganizationUniqueID, false);

                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            foreach (var organization in organizationList)
                            {
                                if (upStreamOrganizationList.Any(x => x == organization) || downStreamOrganizationList.Any(x => x == organization))
                                {
                                    var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var equipment in equipmentList)
                                    {
                                        if (!EquipmentList.Any(x => x.EquipmentUniqueID == equipment.UNIQUEID && x.PartUniqueID == "*"))
                                        {
                                            var equipmentModel = new EquipmentModel()
                                            {
                                                EquipmentUniqueID = equipment.UNIQUEID,
                                                PartUniqueID = "*",
                                                EquipmentID = equipment.ID,
                                                EquipmentName = equipment.NAME,
                                                PartDescription = "",
                                            };

                                            var standardList = (from x in db.EQUIPMENTSTANDARD
                                                                join s in db.STANDARD
                                                                on x.STANDARDUNIQUEID equals s.UNIQUEID
                                                                where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                                select new
                                                                {
                                                                    UniqueID = s.UNIQUEID,
                                                                    MaintenanceType = s.MAINTENANCETYPE,
                                                                    StandardID = s.ID,
                                                                    StandardDescription = s.DESCRIPTION
                                                                }).OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();

                                            foreach (var standard in standardList)
                                            {
                                                equipmentModel.StandardList.Add(new StandardModel()
                                                {
                                                    IsChecked = true,
                                                    UniqueID = standard.UniqueID,
                                                    MaintenanceType = standard.MaintenanceType,
                                                    StandardID = standard.StandardID,
                                                    StandardDescription = standard.StandardDescription
                                                });
                                            }

                                            var materialList = (from x in db.EQUIPMENTMATERIAL
                                                                join m in db.MATERIAL
                                                                on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                                where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                                select new
                                                                {
                                                                    UniqueID = m.UNIQUEID,
                                                                    ID = m.ID,
                                                                    Name = m.NAME,
                                                                    Quantity = x.QUANTITY.Value
                                                                }).OrderBy(x => x.ID).ToList();

                                            foreach (var material in materialList)
                                            {
                                                equipmentModel.MaterialList.Add(new MaterialModel()
                                                {
                                                    IsChecked = false,
                                                    UniqueID = material.UniqueID,
                                                    ID = material.ID,
                                                    Name = material.Name,
                                                    Quantity = material.Quantity
                                                });
                                            }

                                            EquipmentList.Add(equipmentModel);
                                        }

                                        var partList = (from p in db.EQUIPMENTPART
                                                        join e in db.EQUIPMENT
                                                        on p.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                        where p.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                                        select new
                                                        {
                                                            EquipmentUniqueID = e.UNIQUEID,
                                                            PartUniqueID = p.UNIQUEID,
                                                            EquipmentID = e.ID,
                                                            EquipmentName = e.NAME,
                                                            PartDescription = p.DESCRIPTION
                                                        }).OrderBy(x => x.PartDescription).ToList();

                                        foreach (var part in partList)
                                        {
                                            if (!EquipmentList.Any(x => x.EquipmentUniqueID == part.EquipmentUniqueID && x.PartUniqueID == part.PartUniqueID))
                                            {
                                                var equipmentModel = new EquipmentModel()
                                                {
                                                    EquipmentUniqueID = part.EquipmentUniqueID,
                                                    PartUniqueID = part.PartUniqueID,
                                                    EquipmentID = part.EquipmentID,
                                                    EquipmentName = part.EquipmentName,
                                                    PartDescription = part.PartDescription
                                                };

                                                var standardList = (from x in db.EQUIPMENTSTANDARD
                                                                    join s in db.STANDARD
                                                                    on x.STANDARDUNIQUEID equals s.UNIQUEID
                                                                    where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                                    select new
                                                                    {
                                                                        UniqueID = s.UNIQUEID,
                                                                        MaintenanceType = s.MAINTENANCETYPE,
                                                                        StandardID = s.ID,
                                                                        StandardDescription = s.DESCRIPTION
                                                                    }).OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();

                                                foreach (var standard in standardList)
                                                {
                                                    equipmentModel.StandardList.Add(new StandardModel()
                                                    {
                                                        IsChecked = true,
                                                        UniqueID = standard.UniqueID,
                                                        MaintenanceType = standard.MaintenanceType,
                                                        StandardID = standard.StandardID,
                                                        StandardDescription = standard.StandardDescription
                                                    });
                                                }

                                                var materialList = (from x in db.EQUIPMENTMATERIAL
                                                                    join m in db.MATERIAL
                                                                    on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                                    where x.EQUIPMENTUNIQUEID == equipmentModel.EquipmentUniqueID && x.PARTUNIQUEID == equipmentModel.PartUniqueID
                                                                    select new
                                                                    {
                                                                        UniqueID = m.UNIQUEID,
                                                                        ID = m.ID,
                                                                        Name = m.NAME,
                                                                        Quantity = x.QUANTITY.Value
                                                                    }).OrderBy(x => x.ID).ToList();

                                                foreach (var material in materialList)
                                                {
                                                    equipmentModel.MaterialList.Add(new MaterialModel()
                                                    {
                                                        IsChecked = false,
                                                        UniqueID = material.UniqueID,
                                                        ID = material.ID,
                                                        Name = material.Name,
                                                        Quantity = material.Quantity
                                                    });
                                                }

                                                EquipmentList.Add(equipmentModel);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var equipment in EquipmentList)
                {
                    equipment.StandardList = equipment.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();
                    equipment.MaterialList = equipment.MaterialList.OrderBy(x => x.ID).ToList();
                }

                result.ReturnData(EquipmentList.OrderBy(x => x.EquipmentUniqueID).ThenBy(x => x.PartDescription).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddUser(List<Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel> UserList, List<string> SelectedList)
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
                                              select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.NAME,
                                                  OrganizationDescription = o.DESCRIPTION
                                              }).First());
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

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
                                    UserList.Add(new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel()
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var jobList = db.MJOB.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                            ||
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MJOB.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                var treeItem = new TreeItem() { Title = organization.Description };

                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                foreach (var attribute in attributes)
                {
                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MJOB.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }
                }

                treeItemList.Add(treeItem);

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
    }
}
