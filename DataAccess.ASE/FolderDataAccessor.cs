using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.EquipmentMaintenance.FolderManagement;

namespace DataAccess.ASE
{
    public class FolderDataAccessor
    {
        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.FOLDER.Add(new FOLDER()
                    {
                        UNIQUEID = Guid.NewGuid().ToString(),
                        ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                        EQUIPMENTUNIQUEID = Model.EquipmentUniqueID,
                        PARTUNIQUEID = Model.PartUniqueID,
                        MATERIALUNIQUEID = Model.MaterialUniqueID,
                        FOLDERUNIQUEID = Model.FolderUniqueID,
                        DESCRIPTION = Model.FormInput.Description,
                        LASTMODIFYTIME = DateTime.Now
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var folder = db.FOLDER.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = folder.UNIQUEID,
                        FormInput = new FormInput()
                        {
                            Description = folder.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var folder = db.FOLDER.First(x => x.UNIQUEID == Model.UniqueID);

                    folder.DESCRIPTION = Model.FormInput.Description;
                    folder.LASTMODIFYTIME = DateTime.Now;

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
                using (ASEDbEntities db = new ASEDbEntities())
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
