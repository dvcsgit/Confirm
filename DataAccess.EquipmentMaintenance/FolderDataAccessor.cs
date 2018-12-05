using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.FolderManagement;

namespace DataAccess.EquipmentMaintenance
{
    public class FolderDataAccessor
    {
        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    db.Folder.Add(new Folder()
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                        OrganizationUniqueID = Model.OrganizationUniqueID,
                        EquipmentUniqueID = Model.EquipmentUniqueID,
                        PartUniqueID = Model.PartUniqueID,
                        MaterialUniqueID = Model.MaterialUniqueID,
                        FolderUniqueID = Model.FolderUniqueID,
                        Description = Model.FormInput.Description,
                        LastModifyTime = DateTime.Now
                    });

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Folder, Resources.Resource.Success));
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
                using (EDbEntities db = new EDbEntities())
                {
                    var folder = db.Folder.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = folder.UniqueID,
                        FormInput = new FormInput()
                        {
                            Description = folder.Description
                        }
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var folder = db.Folder.First(x => x.UniqueID == Model.UniqueID);

                    folder.Description = Model.FormInput.Description;
                    folder.LastModifyTime = DateTime.Now;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Folder, Resources.Resource.Success));
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
                    DeleteHelper.Folder(db, UniqueID);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Folder, Resources.Resource.Success));
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
