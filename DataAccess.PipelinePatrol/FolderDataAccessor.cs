using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE.PipelinePatrol;
#else
using DbEntity.MSSQL.PipelinePatrol;
#endif
using Models.PipelinePatrol.FolderManagement;
using System.Collections.Generic;

namespace DataAccess.PipelinePatrol
{
    public class FolderDataAccessor
    {
        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    db.Folder.Add(new Folder()
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                        OrganizationUniqueID = Model.OrganizationUniqueID,
                        PipelineUniqueID = Model.PipelineUniqueID,
                        PipePointUniqueID = Model.PipePointUniqueID,
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
                using (PDbEntities db = new PDbEntities())
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
                using (PDbEntities db = new PDbEntities())
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
                using (PDbEntities db = new PDbEntities())
                {
                    Delete(db, UniqueID);

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

        private static void Delete(PDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                Delete(DB, uniqueID);
            }
        }

        private static void Delete(PDbEntities DB, string UniqueID)
        {
            DB.Folder.Remove(DB.Folder.First(x => x.UniqueID == UniqueID));

            Delete(DB, DB.Folder.Where(x => x.FolderUniqueID == UniqueID).Select(x => x.UniqueID).ToList());

            FileDataAccessor.Delete(DB, DB.File.Where(x => x.FolderUniqueID == UniqueID).Select(x => x.UniqueID).ToList());
        }
    }
}
