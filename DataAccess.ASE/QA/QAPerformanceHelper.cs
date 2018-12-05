using DbEntity.ASE;
using Models.ASE.QA.QAPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class QAPerformanceHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<GridItem>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var calibrationApplyFlowList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0).ToList();
                    var calibrationNotifyFlowList = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0).ToList();
                    var calibrationFormFlowList = db.QA_CALIBRATIONFORMFLOWLOG.Where(x => x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0).ToList();
                    var msaNotifyFlowList = (from x in db.QA_MSANOTIFYFLOWLOG
                                         join n in db.QA_MSANOTIFY
                                         on x.NOTIFYUNIQUEID equals n.UNIQUEID
                                         where x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0
                                         select x).ToList();
                    var msaFormFlowList = (from x in db.QA_CALIBRATIONFORMFLOWLOG
                                       join f in db.QA_MSAFORM
                                       on x.FORMUNIQUEID equals f.UNIQUEID
                                       where x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0
                                       select x).ToList();
                    var changeFormFlowList = db.QA_CHANGEFORMFLOWLOG.Where(x => x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0).ToList();
                    var abnormalFormFlowList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.VERIFYTIME.HasValue && DateTime.Compare(x.VERIFYTIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.VERIFYTIME.Value, Parameters.EndDate.Value) <= 0).ToList();

                    var calibrationFormList = db.QA_CALIBRATIONFORM.Where(x => x.CALDATE.HasValue && DateTime.Compare(x.CALDATE.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.CALDATE.Value, Parameters.EndDate.Value) <= 0).ToList();

                    var stepLogList = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.TIME.HasValue && DateTime.Compare(x.TIME.Value, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.TIME.Value, Parameters.EndDate.Value) <= 0).ToList();

                    var userList = (from u in db.ACCOUNT
                                   join x in db.USERAUTHGROUP
                                   on u.ID equals x.USERID
                                   where x.AUTHGROUPID=="QA"
                                   select new{
                                   u.ID,
                                   u.NAME
                                   }).Distinct().OrderBy(x=>x.ID).ToList();

                    foreach (var user in userList)
                    {
                        itemList.Add(new GridItem()
                        {
                            UserID = user.ID,
                            UserName = user.NAME,
                            CalibrationApplyCount = calibrationApplyFlowList.Count(x => x.USERID == user.ID),
                            CalibrationNotifyCount = calibrationNotifyFlowList.Count(x => x.USERID == user.ID),
                            CalibrationFormCount = calibrationFormFlowList.Count(x => x.USERID == user.ID),
                            MSANotifyCount = msaNotifyFlowList.Count(x => x.USERID == user.ID),
                            MSAFormCount = msaFormFlowList.Count(x => x.USERID == user.ID),
                            ChangeFormCount = changeFormFlowList.Count(x => x.USERID == user.ID),
                            AbnormalFormCount = abnormalFormFlowList.Count(x => x.USERID == user.ID),
                            Step1Count = stepLogList.Count(x => x.QAID == user.ID && x.STEP == "1"),
                            Step2Count = stepLogList.Count(x => x.QAID == user.ID && x.STEP == "2"),
                            Step3Count = stepLogList.Count(x => x.QAID == user.ID && x.STEP == "3"),
                            Step4Count = stepLogList.Count(x => x.QAID == user.ID && x.STEP == "4"),
                            CalibrationCount = calibrationFormList.Count(x => x.CALUSERID == user.ID)
                        });
                    }

                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Export(List<GridItem> ItemList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper("校驗人員執行管理", Define.EnumExcelVersion._2007))
                {
                    helper.CreateSheet<GridItem>(ItemList);

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
