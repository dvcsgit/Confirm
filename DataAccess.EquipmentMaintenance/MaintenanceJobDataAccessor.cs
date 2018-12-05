using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.EquipmentMaintenance.MaintenanceJobManagement;
using Models.Authenticated;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class MaintenanceJobDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.MJob.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            Description = x.Description,
                            CycleCount = x.CycleCount,
                            CycleMode = x.CycleMode
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

                using (EDbEntities db = new EDbEntities())
                {
                    var j = db.MJob.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = j.UniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(j.OrganizationUniqueID),
                        Description = j.Description,
                        NotifyDay = j.NotifyDay,
                        BeginDate = j.BeginDate,
                        EndDate = j.EndDate,
                        CycleCount = j.CycleCount,
                        CycleMode = j.CycleMode,
                        Remark = j.Remark,
                        UserList = db.MJobUser.Where(x => x.MJobUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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
                                      select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var job = db.MJob.First(x => x.UniqueID == UniqueID);

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.OrganizationUniqueID),
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            NotifyDay = job.NotifyDay,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.BeginDate),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.EndDate),
                            CycleCount = job.CycleCount,
                            CycleMode = job.CycleMode,
                            Remark = job.Remark
                        },
                        UserList = db.MJobUser.Where(x => x.MJobUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };

                    var equipmentList = (from x in db.MJobEquipment
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         where x.MJobUniqueID == job.UniqueID
                                         select new
                                         {
                                             x = x,
                                             e = e
                                         }).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var part = db.EquipmentPart.FirstOrDefault(x => x.UniqueID == equipment.x.PartUniqueID);

                        var equipmentModel = new EquipmentModel()
                        {
                            EquipmentUniqueID = equipment.x.EquipmentUniqueID,
                            PartUniqueID = equipment.x.PartUniqueID,
                            EquipmentID = equipment.e.ID,
                            EquipmentName = equipment.e.Name,
                            PartDescription = part != null ? part.Description : string.Empty
                        };

                        var equipmentStandardList = (from x in db.EquipmentStandard
                                                     join s in db.Standard
                                                     on x.StandardUniqueID equals s.UniqueID
                                                     where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         s.UniqueID,
                                                         MaintenanceType = s.MaintenanceType,
                                                         s.ID,
                                                         s.Description
                                                     }).ToList();

                        foreach (var standard in equipmentStandardList)
                        {
                            var jobEquipmentStandard = db.MJobEquipmentStandard.FirstOrDefault(x => x.MJobUniqueID == job.UniqueID && x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID && x.StandardUniqueID == standard.UniqueID);

                            equipmentModel.StandardList.Add(new StandardModel()
                            {
                                IsChecked = jobEquipmentStandard != null,
                                UniqueID = standard.UniqueID,
                                MaintenanceType = standard.MaintenanceType,
                                StandardID = standard.ID,
                                StandardDescription = standard.Description
                            });
                        }

                        var equipmentMaterialList = (from x in db.EquipmentMaterial
                                                     join m in db.Material
                                                     on x.MaterialUniqueID equals m.UniqueID
                                                     where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         m.UniqueID,
                                                         m.ID,
                                                         m.Name,
                                                         Quantity = x.Quantity
                                                     }).ToList();

                        foreach (var material in equipmentMaterialList)
                        {
                            var jobEquipmentMaterial = db.MJobEquipmentMaterial.FirstOrDefault(x => x.MJobUniqueID == job.UniqueID && x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID && x.MaterialUniqueID == material.UniqueID);

                            equipmentModel.MaterialList.Add(new MaterialModel()
                            {
                                IsChecked = jobEquipmentMaterial != null,
                                UniqueID = material.UniqueID,
                                ID = material.ID,
                                Name = material.Name,
                                Quantity = material.Quantity
                            });
                        }

                        equipmentModel.StandardList = equipmentModel.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();
                        equipmentModel.MaterialList = equipmentModel.MaterialList.OrderBy(x => x.ID).ToList();

                        model.EquipmentList.Add(equipmentModel);
                    }

                    model.EquipmentList = model.EquipmentList.OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();
                }

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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
                    var exists = db.MJob.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        if (DateTime.Compare(Model.FormInput.BeginDate, DateTime.Today) > 0)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.MJob.Add(new MJob()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                Description = Model.FormInput.Description,
                                NotifyDay = Model.FormInput.NotifyDay,
                                CycleCount = Model.FormInput.CycleCount,
                                CycleMode = Model.FormInput.CycleMode,
                                BeginDate = Model.FormInput.BeginDate,
                                EndDate = Model.FormInput.EndDate,
                                Remark = Model.FormInput.Remark,
                                LastModifyTime = DateTime.Now
                            });

                            db.MJobUser.AddRange(Model.UserList.Select(x => new MJobUser
                            {
                                MJobUniqueID = uniqueID,
                                UserID = x.ID
                            })).ToList();

                            foreach (var equipment in Model.EquipmentList)
                            {
                                if (equipment.StandardList.Count(x => x.IsChecked) > 0 || equipment.MaterialList.Count(x => x.IsChecked && x.ChangeQuantity.HasValue) > 0)
                                {
                                    db.MJobEquipment.Add(new MJobEquipment()
                                    {
                                        MJobUniqueID = uniqueID,
                                        EquipmentUniqueID = equipment.EquipmentUniqueID,
                                        PartUniqueID = equipment.PartUniqueID
                                    });

                                    foreach (var standard in equipment.StandardList)
                                    {
                                        if (standard.IsChecked)
                                        {
                                            db.MJobEquipmentStandard.Add(new MJobEquipmentStandard()
                                            {
                                                MJobUniqueID = uniqueID,
                                                EquipmentUniqueID = equipment.EquipmentUniqueID,
                                                PartUniqueID = equipment.PartUniqueID,
                                                StandardUniqueID = standard.UniqueID
                                            });
                                        }
                                    }

                                    foreach (var material in equipment.MaterialList)
                                    {
                                        if (material.IsChecked && material.ChangeQuantity.HasValue)
                                        {
                                            db.MJobEquipmentMaterial.Add(new MJobEquipmentMaterial()
                                            {
                                                MJobUniqueID = uniqueID,
                                                EquipmentUniqueID = equipment.EquipmentUniqueID,
                                                PartUniqueID = equipment.PartUniqueID,
                                                MaterialUniqueID = material.UniqueID,
                                                Quantity = material.ChangeQuantity
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

                using (EDbEntities db = new EDbEntities())
                {
                    var job = db.MJob.First(x => x.UniqueID == UniqueID);

                    model = new EditFormModel()
                    {
                        UniqueID = job.UniqueID,
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.OrganizationUniqueID),
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                         OrganizationUniqueID=job.OrganizationUniqueID,
                        FormInput = new FormInput()
                        {
                            Description = job.Description,
                            NotifyDay = job.NotifyDay,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.BeginDate),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.EndDate),
                            CycleCount = job.CycleCount,
                            CycleMode = job.CycleMode,
                            Remark = job.Remark
                        },
                        UserList = db.MJobUser.Where(x => x.MJobUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };

                    var equipmentList = (from x in db.MJobEquipment
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         where x.MJobUniqueID == job.UniqueID
                                         select new
                                         {
                                             x = x,
                                             e = e
                                         }).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var part = db.EquipmentPart.FirstOrDefault(x => x.UniqueID == equipment.x.PartUniqueID);

                        var equipmentModel = new EquipmentModel()
                        {
                            EquipmentUniqueID = equipment.x.EquipmentUniqueID,
                            PartUniqueID = equipment.x.PartUniqueID,
                            EquipmentID = equipment.e.ID,
                            EquipmentName = equipment.e.Name,
                            PartDescription = part != null ? part.Description : string.Empty
                        };

                        var equipmentStandardList = (from x in db.EquipmentStandard
                                                     join s in db.Standard
                                                     on x.StandardUniqueID equals s.UniqueID
                                                     where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         s.UniqueID,
                                                         MaintenanceType = s.MaintenanceType,
                                                         s.ID,
                                                         s.Description
                                                     }).ToList();

                        foreach (var standard in equipmentStandardList)
                        {
                            var jobEquipmentStandard = db.MJobEquipmentStandard.FirstOrDefault(x => x.MJobUniqueID == job.UniqueID && x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID && x.StandardUniqueID == standard.UniqueID);

                            equipmentModel.StandardList.Add(new StandardModel()
                            {
                                IsChecked = jobEquipmentStandard != null,
                                UniqueID = standard.UniqueID,
                                MaintenanceType = standard.MaintenanceType,
                                StandardID = standard.ID,
                                StandardDescription = standard.Description,
                                OptionList = db.StandardFeelOption.Where(o => o.StandardUniqueID == standard.UniqueID).Select(o => new FeelOptionModel
                                {
                                    UniqueID = o.UniqueID,
                                    Seq = o.Seq,
                                    Description = o.Description,
                                    IsAbnormal = o.IsAbnormal
                                }).OrderBy(o => o.Seq).ToList()
                            });
                        }

                        var equipmentMaterialList = (from x in db.EquipmentMaterial
                                                     join m in db.Material
                                                     on x.MaterialUniqueID equals m.UniqueID
                                                     where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                     select new
                                                     {
                                                         m.UniqueID,
                                                         m.ID,
                                                         m.Name,
                                                         Quantity = x.Quantity
                                                     }).ToList();

                        foreach (var material in equipmentMaterialList)
                        {
                            var jobEquipmentMaterial = db.MJobEquipmentMaterial.FirstOrDefault(x => x.MJobUniqueID == job.UniqueID && x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID && x.MaterialUniqueID == material.UniqueID);

                            equipmentModel.MaterialList.Add(new MaterialModel()
                            {
                                IsChecked = jobEquipmentMaterial != null,
                                UniqueID = material.UniqueID,
                                ID = material.ID,
                                Name = material.Name,
                                Quantity = material.Quantity
                            });
                        }

                        equipmentModel.StandardList = equipmentModel.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();
                        equipmentModel.MaterialList = equipmentModel.MaterialList.OrderBy(x => x.ID).ToList();

                        model.EquipmentList.Add(equipmentModel);
                    }

                    model.EquipmentList = model.EquipmentList.OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();
                }

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
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
                    var job = db.MJob.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.MJob.FirstOrDefault(x => x.UniqueID != job.UniqueID && x.OrganizationUniqueID == job.OrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        if (DateTime.Compare(Model.FormInput.BeginDate, DateTime.Today) > 0)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region MJob
                            job.Description = Model.FormInput.Description;
                            job.NotifyDay = Model.FormInput.NotifyDay;
                            job.BeginDate = Model.FormInput.BeginDate;
                            job.EndDate = Model.FormInput.EndDate;
                            job.CycleCount = Model.FormInput.CycleCount;
                            job.CycleMode = Model.FormInput.CycleMode;
                            job.Remark = Model.FormInput.Remark;
                            job.LastModifyTime = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region MJobUser
                            #region Delete
                            db.MJobUser.RemoveRange(db.MJobUser.Where(x => x.MJobUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.MJobUser.AddRange(Model.UserList.Select(x => new MJobUser
                            {
                                MJobUniqueID = job.UniqueID,
                                UserID = x.ID
                            })).ToList();
                            #endregion
                            #endregion

                            #region MJobEquipment, MJobEquipmentStandard
                            #region Delete
                            db.MJobEquipment.RemoveRange(db.MJobEquipment.Where(x => x.MJobUniqueID == job.UniqueID).ToList());
                            db.MJobEquipmentStandard.RemoveRange(db.MJobEquipmentStandard.Where(x => x.MJobUniqueID == job.UniqueID).ToList());
                            db.MJobEquipmentMaterial.RemoveRange(db.MJobEquipmentMaterial.Where(x => x.MJobUniqueID == job.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            foreach (var equipment in Model.EquipmentList)
                            {
                                if (equipment.StandardList.Count(x => x.IsChecked) > 0 || equipment.MaterialList.Count(x => x.IsChecked && x.ChangeQuantity.HasValue) > 0)
                                {
                                    db.MJobEquipment.Add(new MJobEquipment()
                                    {
                                        MJobUniqueID = job.UniqueID,
                                        EquipmentUniqueID = equipment.EquipmentUniqueID,
                                        PartUniqueID = equipment.PartUniqueID
                                    });

                                    foreach (var standard in equipment.StandardList)
                                    {
                                        if (standard.IsChecked)
                                        {
                                            db.MJobEquipmentStandard.Add(new MJobEquipmentStandard()
                                            {
                                                MJobUniqueID = job.UniqueID,
                                                EquipmentUniqueID = equipment.EquipmentUniqueID,
                                                PartUniqueID = equipment.PartUniqueID,
                                                StandardUniqueID = standard.UniqueID
                                            });
                                        }
                                    }

                                    foreach (var material in equipment.MaterialList)
                                    {
                                        if (material.IsChecked && material.ChangeQuantity.HasValue)
                                        {
                                            db.MJobEquipmentMaterial.Add(new MJobEquipmentMaterial()
                                            {
                                                MJobUniqueID = job.UniqueID,
                                                EquipmentUniqueID = equipment.EquipmentUniqueID,
                                                PartUniqueID = equipment.PartUniqueID,
                                                MaterialUniqueID = material.UniqueID,
                                                Quantity = material.ChangeQuantity
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
                using (EDbEntities db = new EDbEntities())
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
                    equipment.StandardList = equipment.StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.StandardID).ToList();
                    equipment.MaterialList = equipment.MaterialList.OrderBy(x => x.ID).ToList();
                }

                result.ReturnData(EquipmentList.OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList());
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

                using (EDbEntities db = new EDbEntities())
                {
                    var equipmentList = (from x in db.MJobEquipment
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         join p in db.EquipmentPart
                                         on x.PartUniqueID equals p.UniqueID into tmpPart
                                         from p in tmpPart.DefaultIfEmpty()
                                         where x.MJobUniqueID == JobUniqueID
                                         select new
                                         {
                                             EquipmentUniqueID = e.UniqueID,
                                             PartUniqueID = p != null ? p.UniqueID : "*",
                                             EquipmentID = e.ID,
                                             EquipmentName = e.Name,
                                             PartDescription = p != null ? p.Description : ""
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

                        if (db.MJobEquipmentStandard.Any(x => x.MJobUniqueID == JobUniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID) ||
                            db.MJobEquipmentMaterial.Any(x => x.MJobUniqueID == JobUniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID))
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

                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(PartUniqueID) && PartUniqueID != "*")
                    {
                        var standardList = (from x in db.MJobEquipmentStandard
                                            join s in db.Standard
                                            on x.StandardUniqueID equals s.UniqueID
                                            where x.MJobUniqueID == JobUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID
                                            select new
                                            {
                                                MaintenanceType = s.MaintenanceType,
                                                s.ID,
                                                s.Description
                                            }).OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.Description);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var materialList = (from x in db.MJobEquipmentMaterial
                                            join m in db.Material
                                            on x.MaterialUniqueID equals m.UniqueID
                                            where x.MJobUniqueID == JobUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID
                                            select new
                                            {
                                                m.ID,
                                                m.Name,
                                                Quantity = x.Quantity.Value
                                            }).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = string.Format("{0}({1})", material.Name, material.Quantity) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
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
                        var standardList = (from x in db.MJobEquipmentStandard
                                            join s in db.Standard
                                            on x.StandardUniqueID equals s.UniqueID
                                            where x.MJobUniqueID == JobUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == "*"
                                            select new
                                            {
                                                MaintenanceType = s.MaintenanceType,
                                                s.ID,
                                                s.Description
                                            }).OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.Description);
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var materialList = (from x in db.MJobEquipmentMaterial
                                            join m in db.Material
                                            on x.MaterialUniqueID equals m.UniqueID
                                            where x.MJobUniqueID == JobUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == "*"
                                            select new
                                            {
                                                m.ID,
                                                m.Name,
                                                Quantity = x.Quantity.Value
                                            }).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = string.Format("{0}({1})", material.Name, material.Quantity) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
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
                using (EDbEntities db = new EDbEntities())
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
                                var equipment = (from p in db.EquipmentPart
                                                 join e in db.Equipment
                                                 on p.EquipmentUniqueID equals e.UniqueID
                                                 where p.UniqueID == partUniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UniqueID,
                                                     PartUniqueID = p.UniqueID,
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.Name,
                                                     PartDescription = p.Description
                                                 }).First();

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                    PartUniqueID = equipment.PartUniqueID,
                                    EquipmentID = equipment.EquipmentID,
                                    EquipmentName = equipment.EquipmentName,
                                    PartDescription = equipment.PartDescription
                                };

                                var standardList = (from x in db.EquipmentStandard
                                                    join s in db.Standard
                                                    on x.StandardUniqueID equals s.UniqueID
                                                    where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = s.UniqueID,
                                                        MaintenanceType = s.MaintenanceType,
                                                        StandardID = s.ID,
                                                        StandardDescription = s.Description
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

                                var materialList = (from x in db.EquipmentMaterial
                                                    join m in db.Material
                                                    on x.MaterialUniqueID equals m.UniqueID
                                                    where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UniqueID,
                                                        ID = m.ID,
                                                        Name = m.Name,
                                                        Quantity = x.Quantity
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
                        else if (!string.IsNullOrEmpty(equipmentUniqueID))
                        {
                            if (!EquipmentList.Any(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == "*"))
                            {
                                var equipment = db.Equipment.First(x => x.UniqueID == equipmentUniqueID);

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.UniqueID,
                                    PartUniqueID = "*",
                                    EquipmentID = equipment.ID,
                                    EquipmentName = equipment.Name,
                                    PartDescription = "",
                                };

                                var standardList = (from x in db.EquipmentStandard
                                                    join s in db.Standard
                                                    on x.StandardUniqueID equals s.UniqueID
                                                    where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = s.UniqueID,
                                                        MaintenanceType = s.MaintenanceType,
                                                        StandardID = s.ID,
                                                        StandardDescription = s.Description
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

                                var materialList = (from x in db.EquipmentMaterial
                                                    join m in db.Material
                                                    on x.MaterialUniqueID equals m.UniqueID
                                                    where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UniqueID,
                                                        ID = m.ID,
                                                        Name = m.Name,
                                                        Quantity = x.Quantity
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
                                    var equipmentList = db.Equipment.Where(x => x.OrganizationUniqueID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var equipment in equipmentList)
                                    {
                                        if (!EquipmentList.Any(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"))
                                        {
                                            var equipmentModel = new EquipmentModel()
                                            {
                                                EquipmentUniqueID = equipment.UniqueID,
                                                PartUniqueID = "*",
                                                EquipmentID = equipment.ID,
                                                EquipmentName = equipment.Name,
                                                PartDescription = "",
                                            };

                                            var standardList = (from x in db.EquipmentStandard
                                                                join s in db.Standard
                                                                on x.StandardUniqueID equals s.UniqueID
                                                                where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                                select new
                                                                {
                                                                    UniqueID = s.UniqueID,
                                                                    MaintenanceType = s.MaintenanceType,
                                                                    StandardID = s.ID,
                                                                    StandardDescription = s.Description
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

                                            var materialList = (from x in db.EquipmentMaterial
                                                                join m in db.Material
                                                                on x.MaterialUniqueID equals m.UniqueID
                                                                where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                                select new
                                                                {
                                                                    UniqueID = m.UniqueID,
                                                                    ID = m.ID,
                                                                    Name = m.Name,
                                                                    Quantity = x.Quantity
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

                                        var partList = (from p in db.EquipmentPart
                                                        join e in db.Equipment
                                                        on p.EquipmentUniqueID equals e.UniqueID
                                                        where p.EquipmentUniqueID == equipment.UniqueID
                                                        select new
                                                        {
                                                            EquipmentUniqueID = e.UniqueID,
                                                            PartUniqueID = p.UniqueID,
                                                            EquipmentID = e.ID,
                                                            EquipmentName = e.Name,
                                                            PartDescription = p.Description
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

                                                var standardList = (from x in db.EquipmentStandard
                                                                    join s in db.Standard
                                                                    on x.StandardUniqueID equals s.UniqueID
                                                                    where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                                    select new
                                                                    {
                                                                        UniqueID = s.UniqueID,
                                                                        MaintenanceType = s.MaintenanceType,
                                                                        StandardID = s.ID,
                                                                        StandardDescription = s.Description
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

                                                var materialList = (from x in db.EquipmentMaterial
                                                                    join m in db.Material
                                                                    on x.MaterialUniqueID equals m.UniqueID
                                                                    where x.EquipmentUniqueID == equipmentModel.EquipmentUniqueID && x.PartUniqueID == equipmentModel.PartUniqueID
                                                                    select new
                                                                    {
                                                                        UniqueID = m.UniqueID,
                                                                        ID = m.ID,
                                                                        Name = m.Name,
                                                                        Quantity = x.Quantity
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
                                              select new Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.Name,
                                                  OrganizationDescription = o.Description
                                              }).First());
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

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

                using (EDbEntities db = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var jobList = db.MJob.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UniqueID;

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
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MJob.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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

                using (EDbEntities db = new EDbEntities())
                {
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

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MJob.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
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
    }
}
