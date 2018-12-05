using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.PipelinePatrol;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
#endif
using Models.Shared;
using Models.Authenticated;
using Models.PipelinePatrol.FileManagement;

namespace DataAccess.PipelinePatrol
{
    public class FileDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (PDbEntities db = new PDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from f in db.File
                                 join p in db.PipePoint
                                 on f.PipePointUniqueID equals p.UniqueID into tmpPipePoint
                                 from p in tmpPipePoint.DefaultIfEmpty()
                                 where downStreamOrganizationList.Contains(f.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(f.OrganizationUniqueID)
                                 select new
                                 {
                                     f.UniqueID,
                                     f.FolderUniqueID,
                                     f.PipelineUniqueID,
                                     PipePointType = p != null ? p.PointType : string.Empty,
                                     f.PipePointUniqueID,
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

                    if (!string.IsNullOrEmpty(Parameters.PipePointUniqueID))
                    {
                        query = query.Where(x => x.PipePointUniqueID == Parameters.PipePointUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PipePointType))
                    {
                        query = query.Where(x => x.PipePointType == Parameters.PipePointType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PipelineUniqueID))
                    {
                        query = query.Where(x => x.PipelineUniqueID == Parameters.PipelineUniqueID);
                    }

                    model = new GridViewModel()
                    {
                        Parameters = Parameters,
                        PathDescription = GetFolderDescription(Parameters.OrganizationUniqueID, Parameters.PipelineUniqueID, Parameters.PipePointUniqueID, Parameters.FolderUniqueID),
                        FullPathDescription = GetFolderPath(Parameters.OrganizationUniqueID, Parameters.PipelineUniqueID, Parameters.PipePointUniqueID, Parameters.FolderUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            FullPathDescription = GetFilePath(x.UniqueID),
                            FileName = x.FileName,
                            Extension = x.Extension,
                            Size = x.Size,
                            User = UserDataAccessor.GetUser(x.UserID),
                            LastModifyTime = x.LastModifyTime
                        }).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList()
                    };

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        model.ItemList = model.ItemList.Where(x => x.FileName.Contains(Parameters.Keyword) || x.FullPathDescription.Contains(Parameters.Keyword)).OrderBy(x => x.FullPathDescription).ThenBy(x => x.Display).ToList();
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

                using (PDbEntities db = new PDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = file.UniqueID,
                        FullPathDescription = GetFolderPath(file.OrganizationUniqueID, file.PipelineUniqueID, file.PipePointUniqueID, file.FolderUniqueID),
                        FileName = file.FileName,
                        Extension = file.Extension,
                        IsDownload2Mobile = file.IsDownload2Mobile,
                        Size = file.Size,
                        User = UserDataAccessor.GetUser(file.UserID),
                        LastModifyTime = file.LastModifyTime
                    };
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
                using (PDbEntities db = new PDbEntities())
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
                            PipelineUniqueID = Model.PipelineUniqueID,
                            PipePointUniqueID = Model.PipePointUniqueID,
                            FolderUniqueID = Model.FolderUniqueID,
                            FileName = Model.FormInput.FileName,
                            Extension = Extension,
                            IsDownload2Mobile = Model.FormInput.IsDownload2Mobile,
                            Size = Size,
                            UserID = Account.ID,
                            LastModifyTime = DateTime.Now
                        });

                        db.SaveChanges();

                        using (System.IO.FileStream fs = System.IO.File.Create(System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, UniqueID + ".zip")))
                        {
                            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                            {
                                zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                                ZipHelper.CompressFolder(System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, UniqueID + "." + Extension), zipStream);

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
                using (PDbEntities db = new PDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = file.UniqueID,
                        FullPathDescription = GetFolderPath(file.OrganizationUniqueID, file.PipelineUniqueID, file.PipePointUniqueID, file.FolderUniqueID),
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
                using (PDbEntities db = new PDbEntities())
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
                using (PDbEntities db = new PDbEntities())
                {
                    Delete(db, SelectedList);                  
                    
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
                using (PDbEntities db = new PDbEntities())
                {
                    var file = db.File.First(x => x.UniqueID == UniqueID);

                    result = new FileModel()
                    {
                        UniqueID = file.UniqueID,
                        FileName = file.FileName,
                        Extension = file.Extension,
                        FilePath = Config.PipelinePatrolFileFolderPath
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

        public static RequestResult GetTreeItem(string OrganizationUniqueID, string PipelineUniqueID, string PipePointType, string PipePointUniqueID, string FolderUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.PipelineUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PipePointType, string.Empty },
                    { Define.EnumTreeAttribute.PipePointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.FolderUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.FileUniqueID, string.Empty }
                };

                using (PDbEntities edb = new PDbEntities())
                {
                    //資料夾展開(資料夾.檔案)
                    if (!string.IsNullOrEmpty(FolderUniqueID))
                    {
                        var folderList = edb.Folder.Where(x => x.FolderUniqueID == FolderUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var folder in folderList)
                        {
                            var pipePointType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.PipePointUniqueID))
                            {
                                var pipePoint = edb.PipePoint.FirstOrDefault(x => x.UniqueID == folder.PipePointUniqueID);

                                if (pipePoint != null)
                                {
                                    pipePointType = pipePoint.PointType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = folder.PipelineUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = folder.PipePointUniqueID;
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
                            var pipePointType = string.Empty;

                            if (!string.IsNullOrEmpty(file.PipePointUniqueID))
                            {
                                var pipePoint = edb.PipePoint.FirstOrDefault(x => x.UniqueID == file.PipePointUniqueID);

                                if (pipePoint != null)
                                {
                                    pipePointType = pipePoint.PointType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = file.PipelineUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = file.PipePointUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FolderUniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    //定位點展開(資料夾.檔案)
                    else if (!string.IsNullOrEmpty(PipePointUniqueID))
                    {
                        var folderList = edb.Folder.Where(x => x.PipePointUniqueID == PipePointUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var folder in folderList)
                        {
                            var pipePointType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.PipePointUniqueID))
                            {
                                var pipePoint = edb.PipePoint.FirstOrDefault(x => x.UniqueID == folder.PipePointUniqueID);

                                if (pipePoint != null)
                                {
                                    pipePointType = pipePoint.PointType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = folder.PipelineUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = folder.PipePointUniqueID;
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

                        var fileList = edb.File.Where(x => x.PipePointUniqueID == PipePointUniqueID).OrderBy(x => x.FileName).ToList();

                        foreach (var file in fileList)
                        {
                            var pipePointType = string.Empty;

                            if (!string.IsNullOrEmpty(file.PipePointUniqueID))
                            {
                                var pipePoint = edb.PipePoint.FirstOrDefault(x => x.UniqueID == file.PipePointUniqueID);

                                if (pipePoint != null)
                                {
                                    pipePointType = pipePoint.PointType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = file.PipelineUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = file.PipePointUniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = file.FolderUniqueID;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = file.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    //管線展開(資料夾.檔案)
                    else if (!string.IsNullOrEmpty(PipelineUniqueID))
                    {
                        var folderList = edb.Folder.Where(x => x.PipelineUniqueID == PipelineUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var folder in folderList)
                        {
                            var pipePointType = string.Empty;

                            if (!string.IsNullOrEmpty(folder.PipePointUniqueID))
                            {
                                var pipePoint = edb.PipePoint.FirstOrDefault(x => x.UniqueID == folder.PipePointUniqueID);

                                if (pipePoint != null)
                                {
                                    pipePointType = pipePoint.PointType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = folder.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = folder.PipelineUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = folder.PipePointUniqueID;
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

                        var fileList = edb.File.Where(x => x.PipelineUniqueID == PipelineUniqueID).OrderBy(x => x.FileName).ToList();

                        foreach (var file in fileList)
                        {
                            var pipePointType = string.Empty;

                            if (!string.IsNullOrEmpty(file.PipePointUniqueID))
                            {
                                var pipePoint = edb.PipePoint.FirstOrDefault(x => x.UniqueID == file.PipePointUniqueID);

                                if (pipePoint != null)
                                {
                                    pipePointType = pipePoint.PointType;
                                }
                            }

                            var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = file.PipelineUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = file.PipePointUniqueID;
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
                    else if (!string.IsNullOrEmpty(PipePointType))
                    {
                        var pipePointList = edb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.PointType == PipePointType).OrderBy(x => x.ID).ToList();

                        foreach (var pipePoint in pipePointList)
                        {
                            var treeItem = new TreeItem() { Title = pipePoint.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", pipePoint.ID, pipePoint.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = pipePoint.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePoint.PointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = pipePoint.UniqueID;
                            attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.Folder.Any(x => x.PipePointUniqueID == pipePoint.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)) || edb.File.Any(x => x.PipePointUniqueID == pipePoint.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        using (DbEntities db = new DbEntities())
                        {
                            var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                            foreach (var organization in organizationList)
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                    ||
                                    (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) &&
                                    (edb.Pipeline.Any(x => x.OrganizationUniqueID == organization.UniqueID) ||
                                    edb.PipePoint.Any(x => x.OrganizationUniqueID == organization.UniqueID) ||
                                    edb.Folder.Any(x => x.OrganizationUniqueID == organization.UniqueID && string.IsNullOrEmpty(x.PipelineUniqueID) && string.IsNullOrEmpty(x.PipePointUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)) ||
                                    edb.File.Any(x => x.OrganizationUniqueID == organization.UniqueID && string.IsNullOrEmpty(x.PipelineUniqueID) && string.IsNullOrEmpty(x.PipePointUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)))))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }
                        }

                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var pipelineList = edb.Pipeline.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                            foreach (var pipeline in pipelineList)
                            {
                                var treeItem = new TreeItem() { Title = pipeline.ID };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Pipeline.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = pipeline.ID;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = pipeline.OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = pipeline.UniqueID;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (edb.Folder.Any(x => x.PipelineUniqueID == pipeline.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)) || edb.File.Any(x => x.PipelineUniqueID == pipeline.UniqueID && string.IsNullOrEmpty(x.FolderUniqueID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }

                            var pipePointTypeList = edb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.PointType).Distinct().OrderBy(x => x).ToList();

                            foreach (var pipePointType in pipePointTypeList)
                            {
                                var treeItem = new TreeItem() { Title = pipePointType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePointType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = pipePointType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FolderUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.FileUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }

                            var folderList = edb.Folder.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && string.IsNullOrEmpty(x.PipelineUniqueID) && string.IsNullOrEmpty(x.PipePointUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)).OrderBy(x => x.Description).ToList();

                            foreach (var folder in folderList)
                            {
                                var treeItem = new TreeItem() { Title = folder.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Folder.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = folder.Description;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = folder.OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = folder.PipelineUniqueID;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = folder.PipePointUniqueID;
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

                            var fileList = edb.File.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && string.IsNullOrEmpty(x.PipelineUniqueID) && string.IsNullOrEmpty(x.PipePointUniqueID) && string.IsNullOrEmpty(x.FolderUniqueID)).OrderBy(x => x.FileName).ToList();

                            foreach (var file in fileList)
                            {
                                var treeItem = new TreeItem() { Title = string.Format("{0}.{1}", file.FileName, file.Extension) };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.File.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}.{1}", file.FileName, file.Extension);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = file.OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = file.PipelineUniqueID;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = file.PipePointUniqueID;
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

        private static string GetFolderDescription(string OrganizationUniqueID, string PipelineUniqueID, string PipePointUniqueID, string FolderUniqueID)
        {
            string path = string.Empty;

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    if (!string.IsNullOrEmpty(FolderUniqueID))
                    {
                        path = db.Folder.First(x => x.UniqueID == FolderUniqueID).Description;
                    }
                    else if (!string.IsNullOrEmpty(PipePointUniqueID))
                    {
                        var pipePoint = db.PipePoint.First(x => x.UniqueID == PipePointUniqueID);

                        path = string.Format("{0}/{1}", pipePoint.ID, pipePoint.Name);
                    }
                    else if (!string.IsNullOrEmpty(PipelineUniqueID))
                    {
                        var pipeline = db.Pipeline.First(x => x.UniqueID == PipelineUniqueID);

                        path = pipeline.ID;
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

        private static string GetFolderPath(string OrganizationUniqueID, string PipelineUniqueID, string PipePointUniqueID, string FolderUniqueID)
        {
            string path = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(FolderUniqueID))
                {
                    using (PDbEntities db = new PDbEntities())
                    {
                        var folder = db.Folder.First(x => x.UniqueID == FolderUniqueID);

                        path = folder.Description;

                        while (!string.IsNullOrEmpty(folder.FolderUniqueID))
                        {
                            folder = db.Folder.First(x => x.UniqueID == folder.FolderUniqueID);

                            path = folder.Description + " -> " + path;
                        }
                    }

                    path = GetFolderPath(OrganizationUniqueID, PipelineUniqueID, PipePointUniqueID, "") + " -> " + path;
                }
                else if (!string.IsNullOrEmpty(PipePointUniqueID))
                {
                    path = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID) + " -> " + GetFolderDescription(OrganizationUniqueID, "", PipePointUniqueID, "");
                }
                else if (!string.IsNullOrEmpty(PipelineUniqueID))
                {
                    path = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID) + " -> " + GetFolderDescription(OrganizationUniqueID, PipelineUniqueID, "", "");
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
                using (PDbEntities db = new PDbEntities())
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

                        path = GetFolderPath(file.OrganizationUniqueID, file.PipelineUniqueID, file.PipePointUniqueID, "") + " -> " + path;
                    }
                    else if (!string.IsNullOrEmpty(file.PipePointUniqueID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.OrganizationUniqueID) + " -> " + GetFolderDescription(file.OrganizationUniqueID, "", file.PipePointUniqueID, "");
                    }
                    else if (!string.IsNullOrEmpty(file.PipelineUniqueID))
                    {
                        path = OrganizationDataAccessor.GetOrganizationFullDescription(file.OrganizationUniqueID) + " -> " + GetFolderDescription(file.OrganizationUniqueID, file.PipelineUniqueID, "", "");
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

        public static void Delete(PDbEntities DB, List<string> KeyList)
        {
            foreach (var uniqueID in KeyList)
            {
                var file = DB.File.First(x => x.UniqueID == uniqueID);

                DB.File.Remove(file);

                try
                {
                    var filePath = System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, file.UniqueID + "." + file.Extension);
                    var zipFilePath = System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, file.UniqueID + ".zip");

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    if (System.IO.File.Exists(zipFilePath))
                    {
                        System.IO.File.Delete(zipFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                }
            }
        }
    }
}
