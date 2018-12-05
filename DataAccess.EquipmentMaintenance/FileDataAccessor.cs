using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.FileManagement;

namespace DataAccess.EquipmentMaintenance
{
    public class FileDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from f in db.File
                                 join m in db.Material
                                 on f.MaterialUniqueID equals m.UniqueID into tmpMaterial
                                 from m in tmpMaterial.DefaultIfEmpty()
                                 where downStreamOrganizationList.Contains(f.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(f.OrganizationUniqueID)
                                 select new
                                 {
                                     f.FolderUniqueID,
                                     f.MaterialUniqueID,
                                     MaterialType = m != null ? m.EquipmentType : string.Empty,
                                     f.PartUniqueID,
                                     f.EquipmentUniqueID,
                                     f.UniqueID,
                                     f.FileName,
                                     f.Extension,
                                     f.Size,
                                     f.UserID,
                                     f.LastModifyTime
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
                            Size = x.Size,
                            UserID = x.UserID,
                            LastModifyTime = x.LastModifyTime
                        }).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList()
                    };

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        model.ItemList = model.ItemList.Where(x => x.FileName.Contains(Parameters.Keyword) || x.FullPathDescription.Contains(Parameters.Keyword)).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList();
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    model.ItemList = (from x in model.ItemList
                                      join u in db.User
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
                                          UserName = u != null ? u.Name : "",
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

                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = file.UniqueID,
                        FullPathDescription = GetFolderPath(file.OrganizationUniqueID, file.EquipmentUniqueID, file.PartUniqueID, file.MaterialUniqueID, file.FolderUniqueID),
                        FileName = file.FileName,
                        Extension = file.Extension,
                        IsDownload2Mobile = file.IsDownload2Mobile,
                        Size = file.Size,
                        UserID = file.UserID,
                        LastModifyTime = file.LastModifyTime
                    };
                }

                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.FirstOrDefault(x => x.ID == model.UserID);

                    if (user != null)
                    {
                        model.UserName = user.Name;
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
                using (EDbEntities db = new EDbEntities())
                {
                    if (Model.FormInput.IsDownload2Mobile && Extension != "pdf")
                    {
                        result.ReturnFailedMessage(Resources.Resource.MobileFileMustBePDF);
                    }
                    else
                    {
                        db.File.Add(new File()
                        {
                            UniqueID = UniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            EquipmentUniqueID = Model.EquipmentUniqueID,
                            PartUniqueID = Model.PartUniqueID,
                            MaterialUniqueID = Model.MaterialUniqueID,
                            FolderUniqueID = Model.FolderUniqueID,
                            FileName = Model.FormInput.FileName,
                            Extension = Extension,
                            IsDownload2Mobile = Model.FormInput.IsDownload2Mobile,
                            Size = Size,
                            UserID = Account.ID,
                            LastModifyTime = DateTime.Now
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
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = file.UniqueID,
                        FullPathDescription = GetFolderPath(file.OrganizationUniqueID, file.EquipmentUniqueID, file.PartUniqueID, file.MaterialUniqueID, file.FolderUniqueID),
                        Size = file.Size,
                        FormInput = new FormInput()
                        {
                            FileName = file.FileName,
                            IsDownload2Mobile = file.IsDownload2Mobile
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
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == Model.UniqueID);

                    if (Model.FormInput.IsDownload2Mobile && file.Extension != "pdf")
                    {
                        result.ReturnFailedMessage(Resources.Resource.MobileFileMustBePDF);
                    }
                    else
                    {
                        file.FileName = Model.FormInput.FileName;
                        file.IsDownload2Mobile = Model.FormInput.IsDownload2Mobile;
                        file.UserID = Account.ID;
                        file.LastModifyTime = DateTime.Now;

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
                using (EDbEntities db = new EDbEntities())
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
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == UniqueID);

                    result = new FileModel()
                    {
                        UniqueID = file.UniqueID,
                        FileName = file.FileName,
                        Extension = file.Extension,
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

                using (EDbEntities edb = new EDbEntities())
                {
                    //資料夾展開(資料夾.檔案)
                    if (!string.IsNullOrEmpty(FolderUniqueID))
                    {
                        var folderList = edb.Folder.Where(x => x.FolderUniqueID == FolderUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var folder in folderList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.MaterialUniqueID))
                            {
                                var material = edb.Material.FirstOrDefault(x => x.UniqueID == folder.MaterialUniqueID);

                                if (material != null)
                                {
                                    materialType = material.EquipmentType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PartUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MaterialUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.File.Any(x => x.FolderUniqueID == folder.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        var fileList = edb.File.Where(x => x.FolderUniqueID == FolderUniqueID).OrderBy(x => x.FileName).ToList();

                        foreach (var file in fileList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(file.MaterialUniqueID))
                            {
                                var material = edb.Material.FirstOrDefault(x => x.UniqueID == file.MaterialUniqueID);

                                if (material != null)
                                {
                                    materialType = material.EquipmentType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PartUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MaterialUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FolderUniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UniqueID;

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
                        var folderList = edb.Folder.Where(x => x.MaterialUniqueID == MaterialUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var folder in folderList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.MaterialUniqueID))
                            {
                                var material = edb.Material.FirstOrDefault(x => x.UniqueID == folder.MaterialUniqueID);

                                if (material != null)
                                {
                                    materialType = material.EquipmentType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PartUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MaterialUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.File.Any(x => x.FolderUniqueID == folder.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        var fileList = edb.File.Where(x => x.MaterialUniqueID == MaterialUniqueID).OrderBy(x => x.FileName).ToList();

                        foreach (var file in fileList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(file.MaterialUniqueID))
                            {
                                var material = edb.Material.FirstOrDefault(x => x.UniqueID == file.MaterialUniqueID);

                                if (material != null)
                                {
                                    materialType = material.EquipmentType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PartUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MaterialUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FolderUniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UniqueID;

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
                        var folderList = edb.Folder.Where(x => x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var folder in folderList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.MaterialUniqueID))
                            {
                                var material = edb.Material.FirstOrDefault(x => x.UniqueID == folder.MaterialUniqueID);

                                if (material != null)
                                {
                                    materialType = material.EquipmentType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PartUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MaterialUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.File.Any(x => x.FolderUniqueID == folder.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        var fileList = edb.File.Where(x => x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID).OrderBy(x => x.FileName).ToList();

                        foreach (var file in fileList)
                        {
                            var materialType = string.Empty;

                            if (!string.IsNullOrEmpty(file.MaterialUniqueID))
                            {
                                var material = edb.Material.FirstOrDefault(x => x.UniqueID == file.MaterialUniqueID);

                                if (material != null)
                                {
                                    materialType = material.EquipmentType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PartUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MaterialUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FolderUniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UniqueID;

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
                        var materialList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.EquipmentType == MaterialType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.MaterialType] = material.EquipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.Folder.Any(x => x.MaterialUniqueID == material.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)) || edb.File.Any(x => x.MaterialUniqueID == material.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)))
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
                                (edb.Equipment.Any(x => x.OrganizationUniqueID == organization.UniqueID) ||
                                edb.Material.Any(x => x.OrganizationUniqueID == organization.UniqueID) ||
                                edb.Folder.Any(x => x.OrganizationUniqueID == organization.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && string.IsNullOrEmpty(x.MaterialUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)) ||
                                edb.File.Any(x => x.OrganizationUniqueID == organization.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && string.IsNullOrEmpty(x.MaterialUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)))))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }

                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var equipmentList = edb.Equipment.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                            foreach (var equipment in equipmentList)
                            {
                                var treeItem = new TreeItem() { Title = equipment.Name };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.Name);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = equipment.OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = "*";
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (edb.Folder.Any(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*" && string.IsNullOrEmpty(x.FolderUniqueID)) || edb.File.Any(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*" && string.IsNullOrEmpty(x.FolderUniqueID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);

                                var partList = edb.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).OrderBy(x => x.Description).ToList();

                                foreach (var part in partList)
                                {
                                    var partTreeItem = new TreeItem() { Title = string.Format("{0}-{1}", equipment.Name, part.Description) };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.ID, equipment.Name, part.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = equipment.OrganizationUniqueID;
                                    attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;
                                    attributes[Define.EnumTreeAttribute.PartUniqueID] = part.UniqueID;
                                    attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                    foreach (var attribute in attributes)
                                    {
                                        partTreeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                    }

                                    if (edb.Folder.Any(x => x.PartUniqueID == part.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)) || edb.File.Any(x => x.PartUniqueID == part.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)))
                                    {
                                        treeItem.State = "closed";
                                    }

                                    treeItemList.Add(partTreeItem);
                                }
                            }

                            var materialTypeList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).ToList();

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

                            var folderList = edb.Folder.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && string.IsNullOrEmpty(x.MaterialUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)).OrderBy(x => x.Description).ToList();

                            foreach (var folder in folderList)
                            {
                                var treeItem = new TreeItem() { Title = folder.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = folder.EquipmentUniqueID;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = folder.PartUniqueID;
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = folder.MaterialUniqueID;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = folder.UniqueID;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (edb.Folder.Any(x => x.FolderUniqueID == folder.UniqueID) || edb.File.Any(x => x.FolderUniqueID == folder.UniqueID))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }

                            var fileList = edb.File.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && string.IsNullOrEmpty(x.MaterialUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)).OrderBy(x => x.FileName).ToList();

                            foreach (var file in fileList)
                            {
                                var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = file.EquipmentUniqueID;
                                attributes[Define.EnumTreeAttribute.PartUniqueID] = file.PartUniqueID;
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = file.MaterialUniqueID;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FolderUniqueID;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UniqueID;

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

                using (EDbEntities edb = new EDbEntities())
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
                        (edb.Equipment.Any(x => x.OrganizationUniqueID == organization.UniqueID) ||
                        edb.Material.Any(x => x.OrganizationUniqueID == organization.UniqueID) ||
                        edb.Folder.Any(x => x.OrganizationUniqueID == organization.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && string.IsNullOrEmpty(x.MaterialUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)) ||
                        edb.File.Any(x => x.OrganizationUniqueID == organization.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && string.IsNullOrEmpty(x.MaterialUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)))))
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
                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(FolderUniqueID))
                    {
                        path = db.Folder.First(x => x.UniqueID == FolderUniqueID).Description;
                    }
                    else if (!string.IsNullOrEmpty(MaterialUniqueID))
                    {
                        var material = db.Material.First(x => x.UniqueID == MaterialUniqueID);

                        path = string.Format("{0}/{1}", material.ID, material.Name);
                    }
                    else if (!string.IsNullOrEmpty(PartUniqueID))
                    {
                        var part = (from p in db.EquipmentPart
                                    join e in db.Equipment
                                        on p.EquipmentUniqueID equals e.UniqueID
                                    where p.UniqueID == PartUniqueID
                                    select new
                                    {
                                        EquipmentID = e.ID,
                                        EquipmentName = e.Name,
                                        PartDescription = p.Description
                                    }).First();

                        path = string.Format("{0}/{1}-{2}", part.EquipmentID, part.EquipmentName, part.PartDescription);
                    }
                    else if (!string.IsNullOrEmpty(EquipmentUniqueID))
                    {
                        var equipment = db.Equipment.First(x => x.UniqueID == EquipmentUniqueID);

                        path = string.Format("{0}/{1}", equipment.ID, equipment.Name);
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
                    using (EDbEntities db = new EDbEntities())
                    {
                        var folder = db.Folder.First(x => x.UniqueID == FolderUniqueID);

                        path = folder.Description;

                        while (!string.IsNullOrEmpty(folder.FolderUniqueID))
                        {
                            folder = db.Folder.First(x => x.UniqueID == folder.FolderUniqueID);

                            path = folder.Description + " -> " + path;
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
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == FileUniqueID);

                    if (!string.IsNullOrEmpty(file.FolderUniqueID))
                    {
                        var folder = db.Folder.First(x => x.UniqueID == file.FolderUniqueID);

                        path = folder.Description;

                        while (!string.IsNullOrEmpty(folder.FolderUniqueID))
                        {
                            folder = db.Folder.First(x => x.UniqueID == folder.FolderUniqueID);

                            path = folder.Description + " -> " + path;
                        }

                        path = GetFolderPath(file.OrganizationUniqueID, file.EquipmentUniqueID, file.PartUniqueID,file.MaterialUniqueID, "") + " -> " + path;
                    }
                    else if (!string.IsNullOrEmpty(file.MaterialUniqueID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.OrganizationUniqueID) + " -> " + GetFolderDescription(file.OrganizationUniqueID, "","",file.MaterialUniqueID, "");
                    }
                    else if (!string.IsNullOrEmpty(file.PartUniqueID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.OrganizationUniqueID) + " -> " + GetFolderDescription(file.OrganizationUniqueID, file.EquipmentUniqueID, file.PartUniqueID, "","");
                    }
                    else if (!string.IsNullOrEmpty(file.EquipmentUniqueID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.OrganizationUniqueID) + " -> " + GetFolderDescription(file.OrganizationUniqueID, file.EquipmentUniqueID, "","", "");
                    }
                    else
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.OrganizationUniqueID);
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
