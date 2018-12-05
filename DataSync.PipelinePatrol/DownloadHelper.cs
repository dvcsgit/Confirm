using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using ICSharpCode.SharpZipLib.Zip;
using Models.Authenticated;
using Models.PipelinePatrol.DataSync;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using Models.PipelinePatrol.Shared;

namespace DataSync.PipelinePatrol
{
    public class DownloadHelper : IDisposable
    {
        private string Guid;

        private DownloadDataModel DataModel = new DownloadDataModel();

        public RequestResult Generate(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Init();

                if (result.IsSuccess)
                {
                    result = Query(UserID);

                    if (result.IsSuccess)
                    {
                        result = GenerateSQLite();

                        if (result.IsSuccess)
                        {
                            result = GenerateZip();
                        }
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

        private RequestResult Init()
        {
            RequestResult result = new RequestResult();

            try
            {
                Guid = System.Guid.NewGuid().ToString();

                Directory.CreateDirectory(GeneratedFolderPath);

                System.IO.File.Copy(TemplateDbFilePath, GeneratedDbFilePath);

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult Query(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = new List<string>();

                var upStreamOrganizationList = new List<string>();

                var userOrganizationList = new List<UserOrganizationPermission>();

                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(user.OrganizationUniqueID, true);
                    upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(user.OrganizationUniqueID, true);
                    userOrganizationList = OrganizationDataAccessor.GetUserOrganizationPermissionList(user.OrganizationUniqueID);

                    // 抓有行動裝置user的使用者
                    var organizationMap = db.Organization.ToDictionary(x=>x.UniqueID,x=>x.Description);
                    var tempAllUser = db.User.Where(x=> x.IsMobileUser == true).Select(x => new { x.ID, x.Name, x.Title, x.OrganizationUniqueID }).ToList();
                    var tempUser = new List<UserModel>();
                    foreach (var item in tempAllUser)
                    {
                        var newItem = new UserModel()
                        {
                            ID = item.ID,
                            Name = item.Name,
                            Title = item.Title,
                        };

                        if(organizationMap.ContainsKey(item.OrganizationUniqueID))
                        {
                            newItem.OrganizationDescription = organizationMap[item.OrganizationUniqueID];
                        }

                        tempUser.Add(newItem);
                    }
                    DataModel.UserList = tempUser;
                }

                var queryableOrganizationList = userOrganizationList.Where(x => x.Permission == Define.EnumOrganizationPermission.Queryable || x.Permission == Define.EnumOrganizationPermission.Editable).Select(x => x.UniqueID).ToList();

                
                

                using (PDbEntities db = new PDbEntities())
                {
                    DataModel.LastModifyTime = LastModifyTimeHelper.Get(UserID);

                    DataModel.PipelineAbnormalReasonList = db.AbnormalReason.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Distinct().ToList();

                    DataModel.DialogList = db.Dialog.Select(x => new DialogModel
                    {
                        UniqueID = x.UniqueID,
                        Subject = x.Subject,
                        Description = x.Description,
                        PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                        InspectionUniqueID = x.InspectionUniqueID,
                        ConstructionUniqueID = x.ConstructionUniqueID,
                        Extension = x.Extension
                    }).ToList();

                    DataModel.MessageList.AddRange(db.Message.ToList().Select(x => new MessageModel
                    {
                        DialogUniqueID = x.DialogUniqueID,
                        Seq = x.Seq,
                        Message = x.Message1,
                        Extension = x.Extension,
                        MessageTime = x.MessageTime,
                        User = UserHelper.Get(x.UserID)
                    }).ToList());
                    
                    DataModel.ConstructionFirmList = db.ConstructionFirm.ToList();

                    DataModel.ConstructionTypeList = db.ConstructionType.ToList();

                    // ConstructionPhoto
                    DataModel.ConstructionPhotoList = db.ConstructionPhoto.Where(x => !x.IsClosed).Select(x => new ConstructionPhotoModel
                    {
                        ConstructionUniqueID = x.ConstructionUniqueID,
                        Seq = x.Seq,
                        FileName = x.ConstructionUniqueID + "_" + x.Seq + "." + x.Extension
                    }).ToList();

                    //排掉 已結案的
                    DataModel.ConstructionList = db.Construction.Where(x => !x.ClosedTime.HasValue).ToList();

                    DataModel.InspectionList = db.Inspection.ToList();

                    // [InspectionPhoto] 照片
                    DataModel.InspectionPhotoList = db.InspectionPhoto.Select(x => new InspectionPhotoModel
                    {
                        InspectionUniqueID = x.InspectionUniqueID,
                        Seq = x.Seq,
                        FileName = x.InspectionUniqueID + "_" + x.Seq + "." + x.Extension
                    }).ToList();

                    DataModel.InspectionUserList = db.InspectionUser.ToList();

                    DataModel.PipelineAbnormalList = db.PipelineAbnormal.ToList();

                    DataModel.JobList = db.Job.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => new JobModel
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        IsCheckBySeq = x.IsCheckBySeq,
                        IsShowPrevRecord = x.IsShowPrevRecord,
                        RouteList = db.JobRoute.Where(r => r.JobUniqueID == x.UniqueID).Select(r => r.RouteUniqueID).ToList()
                    }).ToList();

                    var routeList = db.Route.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID)).ToList();

                    foreach (var route in routeList)
                    {
                        var routeModel = new RouteModel()
                        {
                            UniqueID = route.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(route.OrganizationUniqueID),
                            ID = route.ID,
                            Name = route.Name,
                            Remark = route.Remark,
                            PipelineList = db.RoutePipeline.Where(x => x.RouteUniqueID == route.UniqueID).Select(x => x.PipelineUniqueID).ToList()
                        };

                        var checkPointList = (from x in db.RouteCheckPoint
                                              join p in db.PipePoint
                                              on x.PipePointUniqueID equals p.UniqueID
                                              where x.RouteUniqueID == route.UniqueID
                                              select new
                                              {
                                                  p.UniqueID,
                                                  p.PointType,
                                                  p.ID,
                                                  p.Name,
                                                  p.LAT,
                                                  p.LNG,
                                                  p.IsFeelItemDefaultNormal,
                                                  x.MinTimeSpan,
                                                  p.Remark,
                                                  x.Seq
                                              }).ToList();

                        foreach (var checkPoint in checkPointList)
                        {
                            var checkPointModel = new CheckPointModel()
                            {
                                UniqueID = checkPoint.UniqueID,
                                ID = checkPoint.ID,
                                Name = checkPoint.Name,
                                IsFeelItemDefaultNormal = checkPoint.IsFeelItemDefaultNormal,
                                LAT = checkPoint.LAT,
                                LNG = checkPoint.LNG,
                                PointType = checkPoint.PointType,
                                MinTimeSpan = checkPoint.MinTimeSpan,
                                Remark = checkPoint.Remark,
                                Seq = checkPoint.Seq
                            };

                            var checkItemList = (from x in db.RouteCheckPointCheckItem
                                                 join p in db.View_PipePointCheckItem
                                                 on new { x.PipePointUniqueID, x.CheckItemUniqueID } equals new { p.PipePointUniqueID, p.CheckItemUniqueID }
                                                 where x.RouteUniqueID == route.UniqueID && x.PipePointUniqueID == checkPoint.UniqueID
                                                 select new
                                                 {
                                                     UniqueID = p.CheckItemUniqueID,
                                                     p.ID,
                                                     p.Description,
                                                     p.IsFeelItem,
                                                     p.LowerLimit,
                                                     p.LowerAlertLimit,
                                                     p.UpperAlertLimit,
                                                     p.UpperLimit,
                                                     p.Remark,
                                                     p.Unit,
                                                     Seq = x.Seq
                                                 }).ToList();

                            foreach (var checkItem in checkItemList)
                            {
                                checkPointModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    UniqueID = checkItem.UniqueID,
                                    ID = checkItem.ID,
                                    Description = checkItem.Description,
                                    IsFeelItem = checkItem.IsFeelItem,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Remark = checkItem.Remark,
                                    Unit = checkItem.Unit,
                                    Seq = checkItem.Seq,
                                    FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                    {
                                        UniqueID = x.UniqueID,
                                        Description = x.Description,
                                        IsAbnormal = x.IsAbnormal,
                                        Seq = x.Seq
                                    }).ToList(),
                                    AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                          join a in db.AbnormalReason
                                                          on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                          where ca.CheckItemUniqueID == checkItem.UniqueID
                                                          select new AbnormalReasonModel
                                                          {
                                                              UniqueID = a.UniqueID,
                                                              ID = a.ID,
                                                              Description = a.Description,
                                                              HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                    join h in db.HandlingMethod
                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                    where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                    select new HandlingMethodModel
                                                                                    {
                                                                                        UniqueID = h.UniqueID,
                                                                                        ID = h.ID,
                                                                                        Description = h.Description
                                                                                    }).ToList()
                                                          }).ToList()
                                });
                            }

                            routeModel.CheckPointList.Add(checkPointModel);
                        }

                        DataModel.RouteList.Add(routeModel);
                    }

                    var routePipelineList = DataModel.RouteList.SelectMany(x => x.PipelineList).Distinct().ToList();

                    var pipelineList = db.Pipeline.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID) || routePipelineList.Contains(x.UniqueID)).Distinct().ToList();

                    foreach (var pipeline in pipelineList)
                    {
                        DataModel.PipelineList.Add(new PipelineModel()
                        {
                            UniqueID = pipeline.UniqueID,
                            ID = pipeline.ID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(pipeline.OrganizationUniqueID),
                            Color = pipeline.Color,
                            Locus = db.PipelineLocus.Where(x => x.PipelineUniqueID == pipeline.UniqueID).ToList(),
                            SpecList = (from x in db.PipelineSpecValue
                                        join s in db.PipelineSpec
                                        on x.SpecUniqueID equals s.UniqueID
                                        join o in db.PipelineSpecOption
                                        on x.SpecOptionUniqueID equals o.UniqueID into tmpSpecOption
                                        from so in tmpSpecOption.DefaultIfEmpty()
                                        where x.PipelineUniqueID == pipeline.UniqueID
                                        select new PipelineSpecModel
                                        {
                                            Spec = s.Description,
                                            SpecType = s.Type,
                                            Seq = x.Seq,
                                            Option = so != null ? so.Description : "",
                                            Input = x.Value
                                        }).ToList()
                        });
                    }

                    // [PipelineAbnormalPhoto] 照片
                    DataModel.PipelineAbnormalPhotoList = db.PipelineAbnormalPhoto.Select(x => new PipelineAbnormalPhotoModel
                    {
                        PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                        Seq = x.Seq,
                        FileName = x.PipelineAbnormalUniqueID + "_" + x.Seq + "." + x.Extension
                    }).ToList();

                    var routePipePointList = DataModel.RouteList.SelectMany(x => x.CheckPointList.Select(p => p.UniqueID)).Distinct().ToList();

                    var pipePointList = db.PipePoint.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID) || routePipePointList.Contains(x.UniqueID)).Distinct().ToList();

                    foreach (var pipePoint in pipePointList)
                    {
                        DataModel.PipePointList.Add(new PipePointModel()
                        {
                            UniqueID = pipePoint.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(pipePoint.OrganizationUniqueID),
                            PointType = pipePoint.PointType,
                            ID = pipePoint.ID,
                            Name = pipePoint.Name,
                            LAT = pipePoint.LAT,
                            LNG = pipePoint.LNG,
                            Remark = pipePoint.Remark,
                            FileList = db.File.Where(x => x.IsDownload2Mobile && x.PipePointUniqueID == pipePoint.UniqueID).Select(x => new PipePointFileModel
                            {
                                UniqueID = x.UniqueID,
                                Name = x.FileName,
                                Extension = x.Extension
                            }).ToList()
                        });
                    }
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult GenerateSQLite()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(this.SQLiteConnString))
                {
                    conn.Open();

                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        #region AbnormalReason, AbnormalReasonHandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                            cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("HandlingMethodUniqueID", "OTHER");

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.AbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("HandlingMethodUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region AbnormalReasonHandlingMethod
                            foreach (var handlingMethod in abnormalReason.HandlingMethodList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                    cmd.Parameters.AddWithValue("HandlingMethodUniqueID", handlingMethod.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region CheckItemAbnormalReason, CheckItemFeelOption
                        foreach (var checkItem in DataModel.CheckItemList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region CheckItemAbnormalReason
                            foreach (var abnormalReason in checkItem.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region CheckItemFeelOption
                            foreach (var feelOption in checkItem.FeelOptionList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemFeelOption (UniqueID, CheckItemUniqueID, Description, IsAbnormal, Seq) VALUES (@UniqueID, @CheckItemUniqueID, @Description, @IsAbnormal, @Seq)";

                                    cmd.Parameters.AddWithValue("UniqueID", feelOption.UniqueID);
                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("Description", feelOption.Description);
                                    cmd.Parameters.AddWithValue("IsAbnormal", feelOption.IsAbnormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("Seq", feelOption.Seq);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region HandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var handlingMethod in DataModel.HandlingMethodList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", handlingMethod.UniqueID);
                                cmd.Parameters.AddWithValue("ID", handlingMethod.ID);
                                cmd.Parameters.AddWithValue("Description", handlingMethod.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Route, RoutePipeline, CheckPoint, CheckItem
                        foreach (var route in DataModel.RouteList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Route (UniqueID, OrganizationDescription, ID, Name, Remark) VALUES (@UniqueID, @OrganizationDescription, @ID, @Name, @Remark)";

                                cmd.Parameters.AddWithValue("UniqueID", route.UniqueID);
                                cmd.Parameters.AddWithValue("OrganizationDescription", route.OrganizationDescription);
                                cmd.Parameters.AddWithValue("ID", route.ID);
                                cmd.Parameters.AddWithValue("Name", route.Name);
                                cmd.Parameters.AddWithValue("Remark", route.Remark);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var pipeline in route.PipelineList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO RoutePipeline (RouteUniqueID, PipelineUniqueID) VALUES (@RouteUniqueID, @PipelineUniqueID)";

                                    cmd.Parameters.AddWithValue("RouteUniqueID", route.UniqueID);
                                    cmd.Parameters.AddWithValue("PipelineUniqueID", pipeline);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            foreach (var checkPoint in route.CheckPointList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckPoint (RouteUniqueID, PipePointUniqueID, PointType, ID, Name, IsFeelItemDefaultNormal, LNG, LAT, MinTimeSpan, Remark, Seq) VALUES (@RouteUniqueID, @PipePointUniqueID, @PointType, @ID, @Name, @IsFeelItemDefaultNormal, @LNG, @LAT, @MinTimeSpan, @Remark, @Seq)";

                                    cmd.Parameters.AddWithValue("RouteUniqueID", route.UniqueID);
                                    cmd.Parameters.AddWithValue("PipePointUniqueID", checkPoint.UniqueID);
                                    cmd.Parameters.AddWithValue("PointType", checkPoint.PointType);
                                    cmd.Parameters.AddWithValue("ID", checkPoint.ID);
                                    cmd.Parameters.AddWithValue("Name", checkPoint.Name);
                                    cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", checkPoint.IsFeelItemDefaultNormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("LNG", checkPoint.LNG);
                                    cmd.Parameters.AddWithValue("LAT", checkPoint.LAT);
                                    cmd.Parameters.AddWithValue("MinTimeSpan", checkPoint.MinTimeSpan);
                                    cmd.Parameters.AddWithValue("Remark", checkPoint.Remark);
                                    cmd.Parameters.AddWithValue("Seq", checkPoint.Seq);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var checkItem in checkPoint.CheckItemList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO CheckItem (RouteUniqueID, PipePointUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) VALUES (@RouteUniqueID, @PipePointUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                        cmd.Parameters.AddWithValue("RouteUniqueID", route.UniqueID);
                                        cmd.Parameters.AddWithValue("PipePointUniqueID", checkPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                        cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                        cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                        cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                        cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                        cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                        cmd.Parameters.AddWithValue("Remark", checkItem.Remark);
                                        cmd.Parameters.AddWithValue("Seq", checkItem.Seq);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Job, JobRoute
                        foreach (var job in DataModel.JobList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Job (UniqueID, ID, Description, IsCheckBySeq, IsShowPrevRecord) VALUES (@UniqueID, @ID, @Description, @IsCheckBySeq, @IsShowPrevRecord)";

                                cmd.Parameters.AddWithValue("UniqueID", job.UniqueID);
                                cmd.Parameters.AddWithValue("ID", job.ID);
                                cmd.Parameters.AddWithValue("Description", job.Description);
                                cmd.Parameters.AddWithValue("IsCheckBySeq", job.IsCheckBySeq ? "Y" : "N");
                                cmd.Parameters.AddWithValue("IsShowPrevRecord", job.IsShowPrevRecord ? "Y" : "N");

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var route in job.RouteList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO JobRoute (JobUniqueID, RouteUniqueID) VALUES (@JobUniqueID, @RouteUniqueID)";

                                    cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                    cmd.Parameters.AddWithValue("RouteUniqueID", route);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion

                        #region Pipeline, PipelineLocus, PipelineSpec
                        foreach (var pipeline in DataModel.PipelineList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Pipeline (UniqueID, OrganizationDescription, ID, Color) VALUES (@UniqueID, @OrganizationDescription, @ID, @Color)";

                                cmd.Parameters.AddWithValue("UniqueID", pipeline.UniqueID);
                                cmd.Parameters.AddWithValue("OrganizationDescription", pipeline.OrganizationDescription);
                                cmd.Parameters.AddWithValue("ID", pipeline.ID);
                                cmd.Parameters.AddWithValue("Color", pipeline.Color);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var location in pipeline.Locus)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO PipelineLocus (PipelineUniqueID, Seq, LNG, LAT) VALUES (@PipelineUniqueID, @Seq, @LNG, @LAT)";

                                    cmd.Parameters.AddWithValue("PipelineUniqueID", pipeline.UniqueID);
                                    cmd.Parameters.AddWithValue("Seq", location.Seq);
                                    cmd.Parameters.AddWithValue("LNG", location.LNG);
                                    cmd.Parameters.AddWithValue("LAT", location.LAT);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            foreach (var spec in pipeline.SpecList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO PipelineSpec (PipelineUniqueID, Seq, SpecType, Spec, Value) VALUES (@PipelineUniqueID, @Seq, @SpecType, @Spec, @Value)";

                                    cmd.Parameters.AddWithValue("PipelineUniqueID", pipeline.UniqueID);
                                    cmd.Parameters.AddWithValue("Seq", spec.Seq);
                                    cmd.Parameters.AddWithValue("SpecType", spec.SpecType);
                                    cmd.Parameters.AddWithValue("Spec", spec.Spec);
                                    cmd.Parameters.AddWithValue("Value", spec.Value);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion

                        #region Dialog
                        foreach (var dialog in DataModel.DialogList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ChatDialog (UniqueID, Subject, Description, PipelineAbnormalUniqueID, InspectionUniqueID, ConstructionUniqueID, Photo) VALUES (@UniqueID, @Subject, @Description, @PipelineAbnormalUniqueID, @InspectionUniqueID, @ConstructionUniqueID, @Photo)";

                                cmd.Parameters.AddWithValue("UniqueID", dialog.UniqueID);
                                cmd.Parameters.AddWithValue("Subject", dialog.Subject);
                                cmd.Parameters.AddWithValue("Description", dialog.Description);
                                cmd.Parameters.AddWithValue("PipelineAbnormalUniqueID", dialog.PipelineAbnormalUniqueID);
                                cmd.Parameters.AddWithValue("InspectionUniqueID", dialog.InspectionUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionUniqueID", dialog.ConstructionUniqueID);
                                cmd.Parameters.AddWithValue("Photo", dialog.Photo);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Message
                        foreach (var message in DataModel.MessageList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ChatMessage (DialogUniqueID, Seq, UserID, UserName, UserOrganizationDescription, UserTitle, Message, MessageTime, Photo) VALUES (@DialogUniqueID, @Seq, @UserID, @UserName, @UserOrganizationDescription, @UserTitle, @Message, @MessageTime, @Photo)";

                                cmd.Parameters.AddWithValue("DialogUniqueID", message.DialogUniqueID);
                                cmd.Parameters.AddWithValue("Seq", message.Seq);
                                cmd.Parameters.AddWithValue("UserID", message.User.ID);
                                cmd.Parameters.AddWithValue("UserName", message.User.Name);
                                cmd.Parameters.AddWithValue("UserOrganizationDescription", message.User.OrganizationDescription);
                                cmd.Parameters.AddWithValue("UserTitle", message.User.Title);
                                cmd.Parameters.AddWithValue("Message", message.Message);
                                cmd.Parameters.AddWithValue("MessageTime", message.MessageTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss));
                                cmd.Parameters.AddWithValue("Photo", message.Photo);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Inspection
                        foreach (var inspection in DataModel.InspectionList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Inspection (UniqueID, VHNO, PipePointUniqueID, ConstructionFirmUniqueID, ConstructionFirmRemark, ConstructionTypeUniqueID, ConstructionTypeRemark, Description, BeginDate, EndDate, LNG, LAT, Address, UserID, CreateTime) VALUES (@UniqueID, @VHNO, @PipePointUniqueID, @ConstructionFirmUniqueID, @ConstructionFirmRemark, @ConstructionTypeUniqueID, @ConstructionTypeRemark, @Description, @BeginDate, @EndDate, @LNG, @LAT, @Address, @UserID, @CreateTime)";

                                cmd.Parameters.AddWithValue("UniqueID", inspection.UniqueID);
                                cmd.Parameters.AddWithValue("VHNO", inspection.VHNO);
                                cmd.Parameters.AddWithValue("PipePointUniqueID", inspection.PipePointUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionFirmUniqueID", inspection.ConstructionFirmUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionFirmRemark", inspection.ConstructionFirmRemark);
                                cmd.Parameters.AddWithValue("ConstructionTypeUniqueID", inspection.ConstructionTypeUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionTypeRemark", inspection.ConstructionTypeRemark);
                                cmd.Parameters.AddWithValue("Description", inspection.Description);
                                cmd.Parameters.AddWithValue("BeginDate", inspection.BeginDate);
                                cmd.Parameters.AddWithValue("EndDate", inspection.EndDate);
                                cmd.Parameters.AddWithValue("LNG", inspection.LNG);
                                cmd.Parameters.AddWithValue("LAT", inspection.LAT);
                                cmd.Parameters.AddWithValue("Address", inspection.Address);
                                cmd.Parameters.AddWithValue("UserID", inspection.CreateUserID);
                                cmd.Parameters.AddWithValue("CreateTime", inspection.CreateTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss));

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region InspectionPhoto
                        foreach (var inspectionPhoto in DataModel.InspectionPhotoList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO InspectionPhoto (InspectionUniqueID, Seq, FileName) VALUES (@InspectionUniqueID, @Seq, @FileName)";

                                cmd.Parameters.AddWithValue("InspectionUniqueID", inspectionPhoto.InspectionUniqueID);
                                cmd.Parameters.AddWithValue("Seq", inspectionPhoto.Seq);
                                cmd.Parameters.AddWithValue("FileName", inspectionPhoto.FileName);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region InspectionUser
                        foreach (var inspectionUser in DataModel.InspectionUserList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO InspectionUser (InspectionUniqueID, UserID, Remark, InspectTime) VALUES (@InspectionUniqueID, @UserID, @Remark, @InspectTime)";

                                cmd.Parameters.AddWithValue("InspectionUniqueID", inspectionUser.InspectionUniqueID);
                                cmd.Parameters.AddWithValue("UserID", inspectionUser.UserID);
                                cmd.Parameters.AddWithValue("Remark", inspectionUser.Remark);
                                cmd.Parameters.AddWithValue("InspectTime", inspectionUser.InspectTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss));

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Construction
                        foreach (var construction in DataModel.ConstructionList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Construction (UniqueID, VHNO, InspectionUniqueID, PipePointUniqueID, ConstructionFirmUniqueID, ConstructionFirmRemark, ConstructionTypeUniqueID, ConstructionTypeRemark, Description, BeginDate, EndDate, LNG, LAT, Address, UserID, CreateTime) VALUES (@UniqueID, @VHNO, @InspectionUniqueID, @PipePointUniqueID, @ConstructionFirmUniqueID, @ConstructionFirmRemark, @ConstructionTypeUniqueID, @ConstructionTypeRemark, @Description, @BeginDate, @EndDate, @LNG, @LAT, @Address, @UserID, @CreateTime)";

                                cmd.Parameters.AddWithValue("UniqueID", construction.UniqueID);
                                cmd.Parameters.AddWithValue("VHNO", construction.VHNO);
                                cmd.Parameters.AddWithValue("InspectionUniqueID", construction.InspectionUniqueID);
                                cmd.Parameters.AddWithValue("PipePointUniqueID", construction.PipePointUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionFirmUniqueID", construction.ConstructionFirmUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionFirmRemark", construction.ConstructionFirmRemark);
                                cmd.Parameters.AddWithValue("ConstructionTypeUniqueID", construction.ConstructionTypeUniqueID);
                                cmd.Parameters.AddWithValue("ConstructionTypeRemark", construction.ConstructionTypeRemark);
                                cmd.Parameters.AddWithValue("Description", construction.Description);
                                cmd.Parameters.AddWithValue("BeginDate", construction.BeginDate);
                                cmd.Parameters.AddWithValue("EndDate", construction.EndDate);
                                cmd.Parameters.AddWithValue("LNG", construction.LNG);
                                cmd.Parameters.AddWithValue("LAT", construction.LAT);
                                cmd.Parameters.AddWithValue("Address", construction.Address);
                                cmd.Parameters.AddWithValue("UserID", construction.CreateUserID);
                                cmd.Parameters.AddWithValue("CreateTime", construction.CreateTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss));

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region ConstructionPhoto
                        foreach (var constructionPhoto in DataModel.ConstructionPhotoList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ConstructionPhoto (ConstructionUniqueID, Seq, FileName) VALUES (@ConstructionUniqueID, @Seq, @FileName)";

                                cmd.Parameters.AddWithValue("ConstructionUniqueID", constructionPhoto.ConstructionUniqueID);
                                cmd.Parameters.AddWithValue("Seq", constructionPhoto.Seq);
                                cmd.Parameters.AddWithValue("FileName", constructionPhoto.FileName);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region ConstructionFirm
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO ConstructionFirm (UniqueID, ID, Name) VALUES (@UniqueID, @ID, @Name)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Name", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var constructionFirm in DataModel.ConstructionFirmList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ConstructionFirm (UniqueID, ID, Name) VALUES (@UniqueID, @ID, @Name)";

                                cmd.Parameters.AddWithValue("UniqueID", constructionFirm.UniqueID);
                                cmd.Parameters.AddWithValue("ID", constructionFirm.ID);
                                cmd.Parameters.AddWithValue("Name", constructionFirm.Name);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region ConstructionType
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO ConstructionType (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var constructionType in DataModel.ConstructionTypeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ConstructionType (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", constructionType.UniqueID);
                                cmd.Parameters.AddWithValue("ID", constructionType.ID);
                                cmd.Parameters.AddWithValue("Description", constructionType.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region PipelineAbnormal
                        foreach (var pipelineAbnormal in DataModel.PipelineAbnormalList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PipelineAbnormal (UniqueID, VHNO, PipePointUniqueID, AbnormalReasonUniqueID, AbnormalReasonRemark, Description, LNG, LAT, Address, UserID, CreateTime) VALUES (@UniqueID, @VHNO, @PipePointUniqueID, @AbnormalReasonUniqueID, @AbnormalReasonRemark, @Description, @LNG, @LAT, @Address, @UserID, @CreateTime)";

                                cmd.Parameters.AddWithValue("UniqueID", pipelineAbnormal.UniqueID);
                                cmd.Parameters.AddWithValue("VHNO", pipelineAbnormal.VHNO);
                                cmd.Parameters.AddWithValue("PipePointUniqueID", pipelineAbnormal.PipePointUniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", pipelineAbnormal.AbnormalReasonUniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonRemark", pipelineAbnormal.AbnormalReasonRemark);
                                cmd.Parameters.AddWithValue("Description", pipelineAbnormal.Description);
                                cmd.Parameters.AddWithValue("LNG", pipelineAbnormal.LNG);
                                cmd.Parameters.AddWithValue("LAT", pipelineAbnormal.LAT);
                                cmd.Parameters.AddWithValue("Address", pipelineAbnormal.Address);
                                cmd.Parameters.AddWithValue("UserID", pipelineAbnormal.CreateUserID);
                                cmd.Parameters.AddWithValue("CreateTime", pipelineAbnormal.CreateTime.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss));

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region PipelineAbnormalPhoto
                        foreach (var pipelineAbnormalPhoto in DataModel.PipelineAbnormalPhotoList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PipelineAbnormalPhoto (PipelineAbnormalUniqueID, Seq, FileName) VALUES (@PipelineAbnormalUniqueID, @Seq, @FileName)";

                                cmd.Parameters.AddWithValue("PipelineAbnormalUniqueID", pipelineAbnormalPhoto.PipelineAbnormalUniqueID);
                                cmd.Parameters.AddWithValue("Seq", pipelineAbnormalPhoto.Seq);
                                cmd.Parameters.AddWithValue("FileName", pipelineAbnormalPhoto.FileName);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region PipePoint
                        foreach (var pipePoint in DataModel.PipePointList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PipePoint (UniqueID, OrganizationDescription, PointType, ID, Name, LNG, LAT, Remark) VALUES (@UniqueID, @OrganizationDescription, @PointType, @ID, @Name, @LNG, @LAT, @Remark)";

                                cmd.Parameters.AddWithValue("UniqueID", pipePoint.UniqueID);
                                cmd.Parameters.AddWithValue("OrganizationDescription", pipePoint.OrganizationDescription);
                                cmd.Parameters.AddWithValue("PointType", pipePoint.PointType);
                                cmd.Parameters.AddWithValue("ID", pipePoint.ID);
                                cmd.Parameters.AddWithValue("Name", pipePoint.Name);
                                cmd.Parameters.AddWithValue("LNG", pipePoint.LNG);
                                cmd.Parameters.AddWithValue("LAT", pipePoint.LAT);
                                cmd.Parameters.AddWithValue("Remark", pipePoint.Remark);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var file in pipePoint.FileList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO PipePointFile (UniqueID, PipePointUniqueID, FileName) VALUES (@UniqueID, @PipePointUniqueID, @FileName)";

                                    cmd.Parameters.AddWithValue("UniqueID", file.UniqueID);
                                    cmd.Parameters.AddWithValue("PipePointUniqueID", pipePoint.UniqueID);
                                    cmd.Parameters.AddWithValue("FileName", file.FileName);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion

                        #region PipelineAbnormalReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO PipelineAbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.PipelineAbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PipelineAbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);
                                
                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region LastModifyTime
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO LastModifyTime (VersionTime) VALUES (@VersionTime)";

                            cmd.Parameters.AddWithValue("VersionTime", DataModel.LastModifyTime);

                            cmd.ExecuteNonQuery();
                        }
                        #endregion

                        #region User

                        foreach (var user in DataModel.UserList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO User (ID, Title, Name,OrganizationDescription) VALUES (@ID, @Title, @Name,@OrganizationDescription)";
                                cmd.Parameters.AddWithValue("ID", user.ID);
                                cmd.Parameters.AddWithValue("Title", user.Title);
                                cmd.Parameters.AddWithValue("Name", user.Name);
                                cmd.Parameters.AddWithValue("OrganizationDescription", user.OrganizationDescription);
                                cmd.ExecuteNonQuery();
                            }
                        }

                 
                        #endregion		

                        trans.Commit();
                    }

                    conn.Close();
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult GenerateZip()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (FileStream fs = System.IO.File.Create(GeneratedZipFilePath))
                {
                    using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                    {
                        zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                        ZipHelper.CompressFolder(GeneratedDbFilePath, zipStream);

                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream

                        zipStream.Close();
                    }
                }

                result.ReturnData(GeneratedZipFilePath);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private string TemplateDbFilePath
        {
            get
            {
                return Path.Combine(Config.PipelinePatrolSQLiteTemplateFolderPath, Define.SQLite_PipelinePatrol);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.PipelinePatrolSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_PipelinePatrol);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_PipelinePatrol);
            }
        }

        private string SQLiteConnString
        {
            get
            {
                return string.Format("Data Source={0};Version=3;", GeneratedDbFilePath);
            }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        ~DownloadHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
