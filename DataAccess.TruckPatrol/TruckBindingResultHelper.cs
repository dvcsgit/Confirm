using DbEntity.MSSQL.TruckPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataAccess.TruckPatrol
{
    public class TruckBindingResultHelper
    {
        public static void Insert(string TruckBindingUniqueID)
        {
            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var arriveRecordList = db.ArriveRecord.Where(x => x.TruckBindingUniqueID == TruckBindingUniqueID).ToList();

                    var truckModel = new Models.TruckPatrol.ResultQuery.TruckModel()
                    {
                        BindingUniqueID = TruckBindingUniqueID,
                        CheckDateList = arriveRecordList.Select(x => x.ArriveDate).Distinct().OrderBy(x => x).ToList(),
                        CheckUserList = arriveRecordList.Select(x => x.UserName).Distinct().OrderBy(x => x).ToList()
                    };

                    var firstTruck = arriveRecordList.FirstOrDefault(x => x.TruckBindingType == "0" || x.TruckBindingType == "1");
                    var secondTruck = arriveRecordList.FirstOrDefault(x => x.TruckBindingType == "2");

                    if (firstTruck != null)
                    {
                        truckModel.OrganizationUniqueID = firstTruck.OrganizationUniqueID;
                        truckModel.OrganizationUniqueID = OrganizationDataAccessor.GetOrganizationDescription(firstTruck.OrganizationUniqueID);
                        truckModel.FirstTruckUniqueID = firstTruck.UniqueID;
                        truckModel.FirstTruckNo = firstTruck.TruckNo;

                        var controlPointList = db.ControlPoint.Where(x => x.TruckUniqueID == firstTruck.TruckUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new Models.TruckPatrol.ResultQuery.ControlPointModel()
                            {
                                TruckBindingUniqueID = TruckBindingUniqueID,
                                ArriveRecordList = arriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new Models.TruckPatrol.ResultQuery.ArriveRecordModel
                                {
                                    UniqueID = x.UniqueID,
                                    ArriveDate = x.ArriveDate,
                                    ArriveTime = x.ArriveTime,
                                    UserID = x.UserID,
                                    UserName = x.UserName,
                                    UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                    UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                    PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                }).ToList(),
                                UniqueID = controlPoint.UniqueID,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description
                            };

                            var controlPointCheckItemList = (from x in db.View_ControlPointCheckItem
                                                             where x.ControlPointUniqueID == controlPoint.UniqueID
                                                             select new
                                                             {
                                                                 UniqueID = x.CheckItemUniqueID,
                                                                 x.ID,
                                                                 x.Description,
                                                                 x.LowerLimit,
                                                                 x.LowerAlertLimit,
                                                                 x.UpperAlertLimit,
                                                                 x.UpperLimit,
                                                                 x.Unit,
                                                                 x.Seq
                                                             }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = new Models.TruckPatrol.ResultQuery.CheckItemModel()
                                {
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.Description,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit
                                };

                                var checkResultList = (from c in db.CheckResult
                                                       join a in db.ArriveRecord
                                                       on c.ArriveRecordUniqueID equals a.UniqueID
                                                       where a.TruckBindingUniqueID == TruckBindingUniqueID && a.ControlPointUniqueID == controlPoint.UniqueID && c.CheckItemUniqueID == checkItem.UniqueID
                                                       select c).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new Models.TruckPatrol.ResultQuery.CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
                                        UniqueID = checkResult.UniqueID,
                                        CheckDate = checkResult.CheckDate,
                                        CheckTime = checkResult.CheckTime,
                                        IsAbnormal = checkResult.IsAbnormal,
                                        IsAlert = checkResult.IsAlert,
                                        Result = checkResult.Result,
                                        LowerLimit = checkResult.LowerLimit,
                                        LowerAlertLimit = checkResult.LowerAlertLimit,
                                        UpperAlertLimit = checkResult.UpperAlertLimit,
                                        UpperLimit = checkResult.UpperLimit,
                                        Unit = checkResult.Unit,
                                        PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
                                        AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new Models.TruckPatrol.ResultQuery.AbnormalReasonModel
                                        {
                                            Description = a.AbnormalReasonDescription,
                                            Remark = a.AbnormalReasonRemark,
                                            HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new Models.TruckPatrol.ResultQuery.HandlingMethodModel
                                            {
                                                Description = h.HandlingMethodDescription,
                                                Remark = h.HandlingMethodRemark
                                            }).ToList()
                                        }).ToList()
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            truckModel.ControlPointList.Add(controlPointModel);
                        }
                    }

                    if (secondTruck != null)
                    {
                        if (firstTruck == null)
                        {
                            truckModel.OrganizationUniqueID = secondTruck.OrganizationUniqueID;
                            truckModel.OrganizationUniqueID = OrganizationDataAccessor.GetOrganizationDescription(secondTruck.OrganizationUniqueID);
                        }

                        truckModel.SecondTruckUniqueID = secondTruck.UniqueID;
                        truckModel.SecondTruckNo = secondTruck.TruckNo;

                        var controlPointList = db.ControlPoint.Where(x => x.TruckUniqueID == secondTruck.TruckUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new Models.TruckPatrol.ResultQuery.ControlPointModel()
                            {
                                TruckBindingUniqueID = TruckBindingUniqueID,
                                ArriveRecordList = arriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new Models.TruckPatrol.ResultQuery.ArriveRecordModel
                                {
                                    UniqueID = x.UniqueID,
                                    ArriveDate = x.ArriveDate,
                                    ArriveTime = x.ArriveTime,
                                    UserID = x.UserID,
                                    UserName = x.UserName,
                                    UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                    UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                    PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                }).ToList(),
                                UniqueID = controlPoint.UniqueID,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description
                            };

                            var controlPointCheckItemList = (from x in db.View_ControlPointCheckItem
                                                             where x.ControlPointUniqueID == controlPoint.UniqueID
                                                             select new
                                                             {
                                                                 UniqueID = x.CheckItemUniqueID,
                                                                 x.ID,
                                                                 x.Description,
                                                                 x.LowerLimit,
                                                                 x.LowerAlertLimit,
                                                                 x.UpperAlertLimit,
                                                                 x.UpperLimit,
                                                                 x.Unit,
                                                                 x.Seq
                                                             }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = new Models.TruckPatrol.ResultQuery.CheckItemModel()
                                {
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.Description,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit
                                };

                                var checkResultList = (from c in db.CheckResult
                                                       join a in db.ArriveRecord
                                                       on c.ArriveRecordUniqueID equals a.UniqueID
                                                       where a.TruckBindingUniqueID == TruckBindingUniqueID && a.ControlPointUniqueID == controlPoint.UniqueID && c.CheckItemUniqueID == checkItem.UniqueID
                                                       select c).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new Models.TruckPatrol.ResultQuery.CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
                                        UniqueID = checkResult.UniqueID,
                                        CheckDate = checkResult.CheckDate,
                                        CheckTime = checkResult.CheckTime,
                                        IsAbnormal = checkResult.IsAbnormal,
                                        IsAlert = checkResult.IsAlert,
                                        Result = checkResult.Result,
                                        LowerLimit = checkResult.LowerLimit,
                                        LowerAlertLimit = checkResult.LowerAlertLimit,
                                        UpperAlertLimit = checkResult.UpperAlertLimit,
                                        UpperLimit = checkResult.UpperLimit,
                                        Unit = checkResult.Unit,
                                        PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
                                        AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new Models.TruckPatrol.ResultQuery.AbnormalReasonModel
                                        {
                                            Description = a.AbnormalReasonDescription,
                                            Remark = a.AbnormalReasonRemark,
                                            HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new Models.TruckPatrol.ResultQuery.HandlingMethodModel
                                            {
                                                Description = h.HandlingMethodDescription,
                                                Remark = h.HandlingMethodRemark
                                            }).ToList()
                                        }).ToList()
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            truckModel.ControlPointList.Add(controlPointModel);
                        }
                    }

                    var truckBindingResult = db.TruckBindingResult.FirstOrDefault(x => x.UniqueID == TruckBindingUniqueID);

                    if (truckBindingResult == null)
                    {
                        truckBindingResult = new TruckBindingResult()
                        {
                            UniqueID = truckModel.BindingUniqueID,
                            FirstTruckUniqueID = truckModel.FirstTruckUniqueID,
                            FirstTruckNo = truckModel.FirstTruckNo,
                            SecondTruckUniqueID = truckModel.SecondTruckUniqueID,
                            SecondTruckNo = truckModel.SecondTruckNo,
                            CheckDate = truckModel.CheckDate,
                            CheckUser = truckModel.CheckUser,
                            CompleteRate = truckModel.CompleteRate,
                            CompleteRateLabelClass = truckModel.LabelClass,
                            IsAbnormal = truckModel.HaveAbnormal,
                            IsAlert = truckModel.HaveAlert,
                            OrganizationUniqueID = truckModel.OrganizationUniqueID,
                            TimeSpan = truckModel.TimeSpan
                        };

                        db.TruckBindingResult.Add(truckBindingResult);
                    }
                    else
                    {
                        truckBindingResult.FirstTruckUniqueID = truckModel.FirstTruckUniqueID;
                        truckBindingResult.FirstTruckNo = truckModel.FirstTruckNo;
                        truckBindingResult.SecondTruckUniqueID = truckModel.SecondTruckUniqueID;
                        truckBindingResult.SecondTruckNo = truckModel.SecondTruckNo;
                        truckBindingResult.CheckDate = truckModel.CheckDate;
                        truckBindingResult.CheckUser = truckModel.CheckUser;
                        truckBindingResult.CompleteRate = truckModel.CompleteRate;
                        truckBindingResult.CompleteRateLabelClass = truckModel.LabelClass;
                        truckBindingResult.IsAbnormal = truckModel.HaveAbnormal;
                        truckBindingResult.IsAlert = truckModel.HaveAlert;
                        truckBindingResult.OrganizationUniqueID = truckModel.OrganizationUniqueID;
                        truckBindingResult.TimeSpan = truckModel.TimeSpan;
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}
