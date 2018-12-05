using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.FileManagement;

namespace DataAccess.ASE
{
    public class FileDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from f in db.FFILE
                                 where downStreamOrganizationList.Contains(f.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(f.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     FolderUniqueID = f.FOLDERUNIQUEID,
                                     MaterialUniqueID = f.MATERIALUNIQUEID,
                                     MaterialType = "",
                                     PartUniqueID = f.PARTUNIQUEID,
                                     EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                     UniqueID = f.UNIQUEID,
                                     FileName = f.FILENAME,
                                     Extension = f.EXTENSION,
                                     Size = f.S,
                                     UserID = f.USERID,
                                     LastModifyTime = f.LASTMODIFYTIME
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.FolderUniqueID))
                    {
                        query = query.Where(x => x.FolderUniqueID == Parameters.FolderUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.MaterialUniqueID))
                    {
                        query = query.Where(x => x.MaterialUniqueID == Parameters.MaterialUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.MaterialType))
                    {
                        query = (from q in query
                                 join m in db.MATERIAL
                                 on q.MaterialUniqueID equals m.UNIQUEID
                                 select new
                                 {
                                     FolderUniqueID = q.FolderUniqueID,
                                     MaterialUniqueID = q.MaterialUniqueID,
                                     MaterialType = m.EQUIPMENTTYPE,
                                     PartUniqueID = q.PartUniqueID,
                                     EquipmentUniqueID = q.EquipmentUniqueID,
                                     UniqueID = q.UniqueID,
                                     FileName = q.FileName,
                                     Extension = q.Extension,
                                     Size = q.Size,
                                     UserID = q.UserID,
                                     LastModifyTime = q.LastModifyTime
                                 }).AsQueryable();

                        query = query.Where(x => x.MaterialType == Parameters.MaterialType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PartUniqueID))
                    {
                        query = query.Where(x => x.PartUniqueID == Parameters.PartUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                    {
                        query = query.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                    }

                    model = new GridViewModel()
                    {
                        Parameters = Parameters,
                        PathDescription = GetFolderDescription(Parameters.OrganizationUniqueID, Parameters.EquipmentUniqueID, Parameters.PartUniqueID, Parameters.MaterialUniqueID, Parameters.FolderUniqueID),
                        FullPathDescription = GetFolderPath(Parameters.OrganizationUniqueID, Parameters.EquipmentUniqueID, Parameters.PartUniqueID, Parameters.MaterialUniqueID, Parameters.FolderUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            FullPathDescription = GetFilePath(x.UniqueID),
                            FileName = x.FileName,
                            Extension = x.Extension,
                            Size = x.Size.Value,
                            UserID = x.UserID,
                            LastModifyTime = x.LastModifyTime.Value
                        }).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList()
                    };

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        model.ItemList = model.ItemList.Where(x => x.FileName.Contains(Parameters.Keyword) || x.FullPathDescription.Contains(Parameters.Keyword)).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList();
                    }

                    model.ItemList = (from x in model.ItemList
                                      join u in db.ACCOUNT
                                      on x.UserID equals u.ID into tmpUser
                                      from u in tmpUser.DefaultIfEmpty()
                                      select new GridItem
                                      {
                                          UniqueID = x.UniqueID,
                                          FullPathDescription = GetFilePath(x.UniqueID),
                                          FileName = x.FileName,
                                          Extension = x.Extension,
                                          Size = x.Size,
                                          UserID = x.UserID,
                                          UserName = u != null ? u.NAME : "",
                                          LastModifyTime = x.LastModifyTime
                                      }).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList();
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.FFILE.First(x => x.UNIQUEID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = file.UNIQUEID,
                        FullPathDescription = GetFolderPath(file.ORGANIZATIONUNIQUEID, file.EQUIPMENTUNIQUEID, file.PARTUNIQUEID, file.MATERIALUNIQUEID, file.FOLDERUNIQUEID),
                        FileName = file.FILENAME,
                        Extension = file.EXTENSION,
                        IsDownload2Mobile = file.ISDOWNLOAD2MOBILE=="Y",
                        Size = file.S.Value,
                        UserID = file.USERID,
                        LastModifyTime = file.LASTMODIFYTIME.Value
                    };

                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == model.UserID);

                    if (user != null)
                    {
                        model.UserName = user.NAME;
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

        public static RequestResult Create(CreateFormModel Model, string UniqueID, string Extension, int Size, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Model.FormInput.IsDownload2Mobile && Extension != "pdf")
                    {
                        result.ReturnFailedMessage(Resources.Resource.MobileFileMustBePDF);
                    }
                    else
                    {
                        db.FFILE.Add(new FFILE()
                        {
                            UNIQUEID = UniqueID,
                            ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                            EQUIPMENTUNIQUEID = Model.EquipmentUniqueID,
                            PARTUNIQUEID = Model.PartUniqueID,
                            MATERIALUNIQUEID = Model.MaterialUniqueID,
                            FOLDERUNIQUEID = Model.FolderUniqueID,
                            FILENAME = Model.FormInput.FileName,
                            EXTENSION = Extension,
                            ISDOWNLOAD2MOBILE = Model.FormInput.IsDownload2Mobile?"Y":"N",
                            S = Size,
                            USERID = Account.ID,
                            LASTMODIFYTIME = DateTime.Now
                        });

                        db.SaveChanges();

                        using (System.IO.FileStream fs = System.IO.File.Create(System.IO.Path.Combine(Config.EquipmentMaintenanceFileFolderPath, UniqueID + ".zip")))
                        {
                            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                            {
                                zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                                ZipHelper.CompressFolder(System.IO.Path.Combine(Config.EquipmentMaintenanceFileFolderPath, UniqueID + "." + Extension), zipStream);

                                zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream

                                zipStream.Close();
                            }
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.File, Resources.Resource.Success));
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.FFILE.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = file.UNIQUEID,
                        FullPathDescription = GetFolderPath(file.ORGANIZATIONUNIQUEID, file.EQUIPMENTUNIQUEID, file.PARTUNIQUEID, file.MATERIALUNIQUEID, file.FOLDERUNIQUEID),
                        Size = file.S.Value,
                        FormInput = new FormInput()
                        {
                            FileName = file.FILENAME,
                            IsDownload2Mobile = file.ISDOWNLOAD2MOBILE=="Y"
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

        public static RequestResult Edit(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.FFILE.First(x => x.UNIQUEID == Model.UniqueID);

                    if (Model.FormInput.IsDownload2Mobile && file.EXTENSION != "pdf")
                    {
                        result.ReturnFailedMessage(Resources.Resource.MobileFileMustBePDF);
                    }
                    else
                    {
                        file.FILENAME = Model.FormInput.FileName;
                        file.ISDOWNLOAD2MOBILE = Model.FormInput.IsDownload2Mobile?"Y":"N";
                        file.USERID = Account.ID;
                        file.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.File, Resources.Resource.Success));
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
                    DeleteHelper.File(db, SelectedList);                  
                    
                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.File, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static FileModel Get(string UniqueID)
        {
            var result = new FileModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.FFILE.First(x => x.UNIQUEID == UniqueID);

                    result = new FileModel()
                    {
                        UniqueID = file.UNIQUEID,
                        FileName = file.FILENAME,
                        Extension = file.EXTENSION,
                        FilePath = Config.EquipmentMaintenanceFileFolderPath
                    };
                }
            }
            catch (Exception ex)
            {
                result = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string MaterialType, string MaterialUniqueID, string FolderUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.MaterialType, string.Empty },
                    { Define.EnumTreeAttribute.MaterialUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.FolderUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.FileUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    //資料夾展開(資料夾.檔案)
                    if (!string.IsNullOrEmpty(FolderUniqueID))
                    {
                        var folderList = db.FOLDER.Where(x => x.FOLDERUNIQUEID == FolderUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var folder in folderList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.MATERIALUNIQUEID))
                            {
                                var material = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID == folder.MATERIALUNIQUEID);

                                if (material != null)
                                {
                                    materialType = material.EQUIPMENTTYPE;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EQUIPMENTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PARTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MATERIALUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UNIQUEID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.FFILE.Any(x => x.FOLDERUNIQUEID == folder.UNIQUEID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        var fileList = db.FFILE.Where(x => x.FOLDERUNIQUEID == FolderUniqueID).OrderBy(x => x.FILENAME).ToList();

                        foreach (var file in fileList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(file.MATERIALUNIQUEID))
                            {
                                var material = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID == file.MATERIALUNIQUEID);

                                if (material != null)
                                {
                                    materialType = material.EQUIPMENTTYPE;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EQUIPMENTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PARTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MATERIALUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FOLDERUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    //材料展開(資料夾.檔案)
                    else if (!string.IsNullOrEmpty(MaterialUniqueID))
                    {
                        var folderList = db.FOLDER.Where(x => x.MATERIALUNIQUEID == MaterialUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var folder in folderList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.MATERIALUNIQUEID))
                            {
                                var material = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID == folder.MATERIALUNIQUEID);

                                if (material != null)
                                {
                                    materialType = material.EQUIPMENTTYPE;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EQUIPMENTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PARTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MATERIALUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UNIQUEID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.FFILE.Any(x => x.FOLDERUNIQUEID == folder.UNIQUEID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        var fileList = db.FFILE.Where(x => x.MATERIALUNIQUEID == MaterialUniqueID).OrderBy(x => x.FILENAME).ToList();

                        foreach (var file in fileList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(file.MATERIALUNIQUEID))
                            {
                                var material = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID == file.MATERIALUNIQUEID);

                                if (material != null)
                                {
                                    materialType = material.EQUIPMENTTYPE;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EQUIPMENTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PARTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MATERIALUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FOLDERUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    //設備展開(資料夾.檔案)
                    else if (!string.IsNullOrEmpty(PartUniqueID))
                    {
                        var folderList = db.FOLDER.Where(x => x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var folder in folderList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.MATERIALUNIQUEID))
                            {
                                var material = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID == folder.MATERIALUNIQUEID);

                                if (material != null)
                                {
                                    materialType = material.EQUIPMENTTYPE;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EQUIPMENTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PARTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MATERIALUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UNIQUEID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.FFILE.Any(x => x.FOLDERUNIQUEID == folder.UNIQUEID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        var fileList = db.FFILE.Where(x => x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID).OrderBy(x => x.FILENAME).ToList();

                        foreach (var file in fileList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(file.MATERIALUNIQUEID))
                            {
                                var material = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID == file.MATERIALUNIQUEID);

                                if (material != null)
                                {
                                    materialType = material.EQUIPMENTTYPE;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EQUIPMENTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PARTUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MATERIALUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FOLDERUNIQUEID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    //材料類別展開
                    else if (!string.IsNullOrEmpty(MaterialType))
                    {
                        var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.EQUIPMENTTYPE == MaterialType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.MaterialType] = material.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UNIQUEID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.FOLDER.Any(x => x.MATERIALUNIQUEID == material.UNIQUEID && string.IsNullOrEmpty(x.FOLDERUNIQUEID)) || db.FFILE.Any(x => x.MATERIALUNIQUEID == material.UNIQUEID && string.IsNullOrEmpty(x.FOLDERUNIQUEID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) &&
                                (db.EQUIPMENT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID) ||
                                db.MATERIAL.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID) ||
                                db.FOLDER.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && string.IsNullOrEmpty(x.MATERIALUNIQUEID) && string.IsNullOrEmpty(x.FOLDERUNIQUEID)) ||
                                db.FFILE.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && string.IsNullOrEmpty(x.MATERIALUNIQUEID) && string.IsNullOrEmpty(x.FOLDERUNIQUEID)))))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                            foreach (var equipment in equipmentList)
                            {
                                var treeItem = new TreeItem() { Title = equipment.NAME };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.NAME);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = equipment.ORGANIZATIONUNIQUEID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UNIQUEID;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = "*";
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.FOLDER.Any(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*" && string.IsNullOrEmpty(x.FOLDERUNIQUEID)) || db.FFILE.Any(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*" && string.IsNullOrEmpty(x.FOLDERUNIQUEID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);

                                var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                                foreach (var part in partList)
                                {
                                    var partTreeItem = new TreeItem() { Title = string.Format("{0}-{1}", equipment.NAME, part.DESCRIPTION) };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.ID, equipment.NAME, part.DESCRIPTION);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = equipment.ORGANIZATIONUNIQUEID;
                                    attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UNIQUEID;
                                    attributes[Define.EnumTreeAttribute.PartUniqueID] = part.UNIQUEID;
                                    attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                    foreach (var attribute in attributes)
                                    {
                                        partTreeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                    }

                                    if (db.FOLDER.Any(x => x.PARTUNIQUEID == part.UNIQUEID && string.IsNullOrEmpty(x.FOLDERUNIQUEID)) || db.FFILE.Any(x => x.PARTUNIQUEID == part.UNIQUEID && string.IsNullOrEmpty(x.FOLDERUNIQUEID)))
                                    {
                                        treeItem.State = "closed";
                                    }

                                    treeItemList.Add(partTreeItem);
                                }
                            }

                            var materialTypeList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).ToList();

                            foreach (var materialType in materialTypeList)
                            {
                                var treeItem = new TreeItem() { Title = materialType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = materialType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }

                            var folderList = db.FOLDER.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && string.IsNullOrEmpty(x.MATERIALUNIQUEID) && string.IsNullOrEmpty(x.FOLDERUNIQUEID)).OrderBy(x => x.DESCRIPTION).ToList();

                            foreach (var folder in folderList)
                            {
                                var treeItem = new TreeItem() { Title = folder.DESCRIPTION };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = folder.DESCRIPTION;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.ORGANIZATIONUNIQUEID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EQUIPMENTUNIQUEID;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PARTUNIQUEID;
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MATERIALUNIQUEID;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UNIQUEID;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.FOLDER.Any(x => x.FOLDERUNIQUEID == folder.UNIQUEID) || db.FFILE.Any(x => x.FOLDERUNIQUEID == folder.UNIQUEID))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }

                            var fileList = db.FFILE.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && string.IsNullOrEmpty(x.MATERIALUNIQUEID) && string.IsNullOrEmpty(x.FOLDERUNIQUEID)).OrderBy(x => x.FILENAME).ToList();

                            foreach (var file in fileList)
                            {
                                var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION) };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.ORGANIZATIONUNIQUEID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EQUIPMENTUNIQUEID;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PARTUNIQUEID;
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MATERIALUNIQUEID;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FOLDERUNIQUEID;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UNIQUEID;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItemList.Add(treeItem);
                            }
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.MaterialType, string.Empty },
                    { Define.EnumTreeAttribute.MaterialUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.FolderUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.FileUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                    attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;
                    attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                    attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                    attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                    attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) &&
                        (db.EQUIPMENT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID) ||
                        db.MATERIAL.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID) ||
                        db.FOLDER.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && string.IsNullOrEmpty(x.MATERIALUNIQUEID) && string.IsNullOrEmpty(x.FOLDERUNIQUEID)) ||
                        db.FFILE.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && string.IsNullOrEmpty(x.MATERIALUNIQUEID) && string.IsNullOrEmpty(x.FOLDERUNIQUEID)))))
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

        private static string GetFolderDescription(string OrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string MaterialUniqueID, string FolderUniqueID)
        {
            string path = string.Empty;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(FolderUniqueID))
                    {
                        path = db.FOLDER.First(x => x.UNIQUEID == FolderUniqueID).DESCRIPTION;
                    }
                    else if (!string.IsNullOrEmpty(MaterialUniqueID))
                    {
                        var material = db.MATERIAL.First(x => x.UNIQUEID == MaterialUniqueID);

                        path = string.Format("{0}/{1}", material.ID, material.NAME);
                    }
                    else if (!string.IsNullOrEmpty(PartUniqueID))
                    {
                        var part = (from p in db.EQUIPMENTPART
                                    join e in db.EQUIPMENT
                                        on p.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                    where p.UNIQUEID == PartUniqueID
                                    select new
                                    {
                                        EquipmentID = e.ID,
                                        EquipmentName = e.NAME,
                                        PartDescription = p.DESCRIPTION
                                    }).First();

                        path = string.Format("{0}/{1}-{2}", part.EquipmentID, part.EquipmentName, part.PartDescription);
                    }
                    else if (!string.IsNullOrEmpty(EquipmentUniqueID))
                    {
                        var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == EquipmentUniqueID);

                        path = string.Format("{0}/{1}", equipment.ID, equipment.NAME);
                    }
                    else
                    {
                        path = OrganizationDataAccessor.GetOrganizationDescription(OrganizationUniqueID);
                    }
                }
            }
            catch (Exception ex)
            {
                path = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return path;
        }

        private static string GetFolderPath(string OrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string MaterialUniqueID, string FolderUniqueID)
        {
            string path = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(FolderUniqueID))
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var folder = db.FOLDER.First(x => x.UNIQUEID == FolderUniqueID);

                        path = folder.DESCRIPTION;

                        while (!string.IsNullOrEmpty(folder.FOLDERUNIQUEID))
                        {
                            folder = db.FOLDER.First(x => x.UNIQUEID == folder.FOLDERUNIQUEID);

                            path = folder.DESCRIPTION + " -> " + path;
                        }
                    }

                    path = GetFolderPath(OrganizationUniqueID, EquipmentUniqueID, PartUniqueID,MaterialUniqueID, "") + " -> " + path;
                }
                else if (!string.IsNullOrEmpty(MaterialUniqueID))
                {
                    path = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID) + " -> " + GetFolderDescription(OrganizationUniqueID, "", "",MaterialUniqueID, "");
                }
                else if (!string.IsNullOrEmpty(PartUniqueID))
                {
                    path = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID) + " -> " + GetFolderDescription(OrganizationUniqueID, EquipmentUniqueID, PartUniqueID, "", "");
                }
                else if (!string.IsNullOrEmpty(EquipmentUniqueID))
                {
                    path = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID) + " -> " + GetFolderDescription(OrganizationUniqueID, EquipmentUniqueID, "","", "");
                }
                else
                {
                    path = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID);
                }

            }
            catch (Exception ex)
            {
                path = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return path;
        }

        private static string GetFilePath(string FileUniqueID)
        {
            string path = string.Empty;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.FFILE.First(x => x.UNIQUEID == FileUniqueID);

                    if (!string.IsNullOrEmpty(file.FOLDERUNIQUEID))
                    {
                        var folder = db.FOLDER.First(x => x.UNIQUEID == file.FOLDERUNIQUEID);

                        path = folder.DESCRIPTION;

                        while (!string.IsNullOrEmpty(folder.FOLDERUNIQUEID))
                        {
                            folder = db.FOLDER.First(x => x.UNIQUEID == folder.FOLDERUNIQUEID);

                            path = folder.DESCRIPTION + " -> " + path;
                        }

                        path = GetFolderPath(file.ORGANIZATIONUNIQUEID, file.EQUIPMENTUNIQUEID, file.PARTUNIQUEID, file.MATERIALUNIQUEID, "") + " -> " + path;
                    }
                    else if (!string.IsNullOrEmpty(file.MATERIALUNIQUEID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.ORGANIZATIONUNIQUEID) + " -> " + GetFolderDescription(file.ORGANIZATIONUNIQUEID, "", "", file.MATERIALUNIQUEID, "");
                    }
                    else if (!string.IsNullOrEmpty(file.PARTUNIQUEID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.ORGANIZATIONUNIQUEID) + " -> " + GetFolderDescription(file.ORGANIZATIONUNIQUEID, file.EQUIPMENTUNIQUEID, file.PARTUNIQUEID, "","");
                    }
                    else if (!string.IsNullOrEmpty(file.EQUIPMENTUNIQUEID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.ORGANIZATIONUNIQUEID) + " -> " + GetFolderDescription(file.ORGANIZATIONUNIQUEID, file.EQUIPMENTUNIQUEID, "","", "");
                    }
                    else
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.ORGANIZATIONUNIQUEID);
                    }
                }
            }
            catch (Exception ex)
            {
                path = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return path;
        }
    }
}
