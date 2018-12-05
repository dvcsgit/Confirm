using Customized.CHIMEI.Models.AIMSJobQuery;
using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace Customized.CHIMEI.DataAccess
{
    public class AIMSJobQueryHelper
    {
        public static RequestResult SaveAbnormalReason(string UniqueID, string AbnormalReason, string HandlingMethod)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    db.CheckResultAbnormalReason.RemoveRange(db.CheckResultAbnormalReason.Where(x => x.CheckResultUniqueID == UniqueID).ToList());
                    db.CheckResultHandlingMethod.RemoveRange(db.CheckResultHandlingMethod.Where(x => x.CheckResultUniqueID == UniqueID).ToList());

                    db.SaveChanges();

                    if (!string.IsNullOrEmpty(AbnormalReason))
                    {
                        db.CheckResultAbnormalReason.Add(new CheckResultAbnormalReason()
                        {
                            CheckResultUniqueID = UniqueID,
                            AbnormalReasonUniqueID = "OTHER",
                            AbnormalReasonID = "OTHER",
                            AbnormalReasonDescription = Resources.Resource.Other,
                            AbnormalReasonRemark = AbnormalReason
                        });

                        if (!string.IsNullOrEmpty(HandlingMethod))
                        {
                            db.CheckResultHandlingMethod.Add(new CheckResultHandlingMethod()
                            {
                                CheckResultUniqueID = UniqueID,
                                AbnormalReasonUniqueID = "OTHER",
                                HandlingMethodUniqueID = "OTHER",
                                HandlingMethodID = "OTHER",
                                HandlingMethodDescription = Resources.Resource.Other,
                                HandlingMethodRemark = HandlingMethod
                            });
                        }
                       
                        db.SaveChanges();
                    }
                }

                result.ReturnSuccessMessage("儲存成功");
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from a in db.CHIMEI_JOB
                                 join j in db.Job
                                 on a.JobUniqueID equals j.UniqueID
                                 join r in db.Route
                                 on j.RouteUniqueID equals r.UniqueID
                                 where !string.IsNullOrEmpty(a.ACT_KEY)
                                 select new
                                 {
                                     VHNO = a.ACT_KEY,
                                     JobDescription = a.ACT_DESC,
                                     JobDate = j.BeginDate,
                                     JobEndDate = j.EndDate,
                                     JobUniqueID = j.UniqueID,
                                     a.EquipmentUniqueID,
                                     a.PartUniqueID,
                                     r.OrganizationUniqueID
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.OrganizationUniqueID))
                    {
                        var downStreamOrganization = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        query = query.Where(x => downStreamOrganization.Contains(x.OrganizationUniqueID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }
                    
                    if (!string.IsNullOrEmpty(Parameters.Cycle))
                    {
                        query = query.Where(x => x.JobDescription.Contains(Parameters.Cycle));
                    }

                    if (!string.IsNullOrEmpty(Parameters.MotorType))
                    {
                        query = query.Where(x => x.JobDescription.Contains(Parameters.MotorType));
                    }

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.JobDate, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.JobDate, Parameters.EndDate.Value) <= 0);
                    }

                    var queryResults = query.Distinct().ToList();

                    var aimsJobs = queryResults.Select(x => new
                    {
                        x.VHNO,
                        x.JobDescription,
                        x.JobDate,
                        x.JobEndDate
                    }).Distinct().ToList();

                    var itemList = new List<AIMSJobModel>();

                    foreach (var aimsJob in aimsJobs)
                    {
                        var aimsJobModel = new AIMSJobModel()
                        {
                            VHNO = aimsJob.VHNO,
                            JobDecsription = aimsJob.JobDescription,
                            JobDate = aimsJob.JobDate,
                            JobEndDate = aimsJob.JobEndDate.Value
                        };

                        var jobList = queryResults.Where(x => x.VHNO == aimsJob.VHNO).Select(x => new
                        {
                            x.JobUniqueID,
                            x.OrganizationUniqueID
                        }).Distinct().ToList();

                        var jobUniqueIDList = jobList.Select(x => x.JobUniqueID).ToList();

                        var allArriveRecordList = db.ArriveRecord.Where(x => jobUniqueIDList.Contains(x.JobUniqueID)).ToList();
                        var allCheckResultList = db.CheckResult.Where(x => jobUniqueIDList.Contains(x.JobUniqueID)).ToList();

                        foreach (var job in jobList)
                        {
                            var jobModel = new JobModel()
                            {
                                UniqueID = job.JobUniqueID,
                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(job.OrganizationUniqueID)
                            };

                            var jobUsers = db.JobUser.Where(x => x.JobUniqueID == job.JobUniqueID).Select(x => x.UserID).ToList();

                            foreach (var jobUser in jobUsers)
                            {
                                jobModel.JobUserList.Add(UserDataAccessor.GetUser(jobUser));
                            }

                            var equipmentList = queryResults.Where(x => x.VHNO == aimsJob.VHNO && x.JobUniqueID == job.JobUniqueID).Select(x => new
                            {
                                x.EquipmentUniqueID,
                                x.PartUniqueID
                            }).ToList();

                            foreach (var equipment in equipmentList)
                            {
                                var equip = db.Equipment.FirstOrDefault(x => x.UniqueID == equipment.EquipmentUniqueID);

                                var equipmentModel = new EquipmentModel()
                                {
                                    UniqueID = equipment.EquipmentUniqueID,
                                    ID = equip!=null?equip.ID:string.Empty,
                                    Name = equip!=null?equip.Name:string.Empty,
                                    ArriveRecordList = allArriveRecordList.Where(x => x.JobUniqueID == job.JobUniqueID && x.ControlPointUniqueID == equipment.EquipmentUniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
                                    {
                                        ArriveDate = x.ArriveDate,
                                        ArriveTime = x.ArriveTime,
                                        ArriveUser = UserDataAccessor.GetUser(x.UserID),
                                        UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                        UnRFIDReasonRemark = x.UnRFIDReasonRemark
                                    }).ToList()
                                };

                                var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
                                                              join j in db.Job
                                                              on x.JobUniqueID equals j.UniqueID
                                                              join y in db.RouteEquipmentCheckItem
                                                              on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                                              join e in db.Equipment
                                                              on x.EquipmentUniqueID equals e.UniqueID
                                                              join p in db.EquipmentPart
                                                              on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                              from p in tmpPart.DefaultIfEmpty()
                                                              join c in db.View_EquipmentCheckItem
                                                              on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                              where x.JobUniqueID == job.JobUniqueID && x.ControlPointUniqueID == equipment.EquipmentUniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                              select new
                                                              {
                                                                  x.EquipmentUniqueID,
                                                                  x.PartUniqueID,
                                                                  PartDescription = p != null ? p.Description : "",
                                                                  x.CheckItemUniqueID,
                                                                  CheckItemID = c.ID,
                                                                  CheckItemDescription = c.Description,
                                                                  c.LowerLimit,
                                                                  c.LowerAlertLimit,
                                                                  c.UpperAlertLimit,
                                                                  c.UpperLimit,
                                                                  c.Unit,
                                                                  y.Seq
                                                              }).OrderBy(x => x.Seq).ToList();

                                foreach (var checkItem in equipmentCheckItemList)
                                {
                                    var checkItemModel = new CheckItemModel()
                                    {
                                        CheckItemID = checkItem.CheckItemID,
                                        CheckItemDescription = checkItem.CheckItemDescription,
                                        LowerLimit = checkItem.LowerLimit,
                                        LowerAlertLimit = checkItem.LowerAlertLimit,
                                        UpperAlertLimit = checkItem.UpperAlertLimit,
                                        UpperLimit = checkItem.UpperLimit,
                                        Unit = checkItem.Unit
                                    };

                                    var checkResultList = allCheckResultList.Where(x => x.JobUniqueID == job.JobUniqueID && x.ControlPointUniqueID == equipment.EquipmentUniqueID && x.EquipmentUniqueID == checkItem.EquipmentUniqueID && x.PartUniqueID == checkItem.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                    foreach (var checkResult in checkResultList)
                                    {
                                        checkItemModel.CheckResultList.Add(new CheckResultModel()
                                        {
                                            UniqueID = checkResult.UniqueID,
                                            CheckDate = checkResult.CheckDate,
                                            CheckTime = checkResult.CheckTime,
                                            IsAbnormal = checkResult.IsAbnormal,
                                            IsAlert = checkResult.IsAlert,
                                            Result = checkResult.Result,
                                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.AbnormalReasonDescription,
                                                Remark = a.AbnormalReasonRemark,
                                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HandlingMethodDescription,
                                                    Remark = h.HandlingMethodRemark
                                                }).ToList()
                                            }).ToList()
                                        });
                                    }

                                    equipmentModel.CheckItemList.Add(checkItemModel);
                                }

                                jobModel.EquipmentList.Add(equipmentModel);
                            }

                            aimsJobModel.JobList.Add(jobModel);
                        }

                        itemList.Add(aimsJobModel);
                    }

                    result.ReturnData(itemList);
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

        public static RequestResult GetTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                        var query = (from j in edb.Job
                                     join r in edb.Route
                                     on j.RouteUniqueID equals r.UniqueID
                                     where downStream.Contains(r.OrganizationUniqueID)
                                     select r).ToList();

                        if (query.Count > 0)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID))
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

        public static RequestResult Export(List<AIMSJobModel> Model, string VHNO)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(VHNO, Define.EnumExcelVersion._2007))
                {
                    var excelItemList = new List<ExcelItem>();

                    var aimsJob = Model.First(x => x.VHNO == VHNO);

                    foreach (var job in aimsJob.JobList)
                    {
                        foreach (var equipment in job.EquipmentList)
                        {
                            foreach (var checkItem in equipment.CheckItemList)
                            {
                                if (checkItem.CheckResultList.Count > 0)
                                {
                                    foreach (var checkResult in checkItem.CheckResultList)
                                    {
                                        excelItemList.Add(new ExcelItem()
                                        {
                                            Abnormal = checkResult.IsAbnormal ? Resources.Resource.Abnormal : (checkResult.IsAlert ? Resources.Resource.Warning : ""),
                                            EquipmentID = equipment.ID,
                                            EquipmentName = equipment.Name,
                                            CheckItemID = checkItem.CheckItemID,
                                            CheckItemDescription = checkItem.CheckItemDescription,
                                            CheckDate = checkResult.CheckDate,
                                            CheckTime = checkResult.CheckTime,
                                            Result = checkResult.Result,
                                            UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                            UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.Value.ToString() : "",
                                            Unit = checkItem.Unit,
                                            AbnormalReasons = checkResult.AbnormalReasons
                                        });
                                    }
                                }
                                else
                                {
                                    excelItemList.Add(new ExcelItem()
                                    {
                                        Abnormal = "",
                                        EquipmentID = equipment.ID,
                                        EquipmentName = equipment.Name,
                                        CheckItemID = checkItem.CheckItemID,
                                        CheckItemDescription = checkItem.CheckItemDescription,
                                        CheckDate = "",
                                        CheckTime = "",
                                        Result = "",
                                        UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                        UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.Value.ToString() : "",
                                        Unit = checkItem.Unit,
                                        AbnormalReasons = ""
                                    });
                                }
                            }
                        }
                    }

                    helper.CreateSheet<ExcelItem>(Resources.Resource.CheckResult, excelItemList);

                    result.ReturnData(helper.Export());
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
    }
}
