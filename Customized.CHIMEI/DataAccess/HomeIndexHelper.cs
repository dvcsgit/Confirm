using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Home;
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
    public class HomeIndexHelper
    {
        public static RequestResult GetAbnormalHandlingList(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                    //個人負責巡檢異常
                    var query1 = (from a in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join y in db.RouteManager
                                  on c.RouteUniqueID equals y.RouteUniqueID
                                  where y.UserID == Account.ID
                                  select new
                                  {
                                      c.EquipmentUniqueID,
                                      a.ClosedTime,
                                      c.CheckDate,
                                      c.IsAbnormal,
                                      c.IsAlert
                                  }).Distinct().ToList();

                    var q11 = query1.Where(x => x.CheckDate == today && x.IsAbnormal).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = q11.Count,
                        Text = "今日異常項目總數",
                        Status = "1"
                    });

                    var q12 = query1.Where(x => x.CheckDate == today && x.IsAlert && !q11.Contains(x.EquipmentUniqueID)).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = q12.Count,
                        Text = "今日注意項目總數",
                        Status = "2"
                    });

                    var q13 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = q13.Count,
                        Text = "累積異常未結案",
                        Status = "3"
                    });

                    var q14 = query1.Where(x => x.IsAlert && !q13.Contains(x.EquipmentUniqueID)).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-exclamation-circle",
                        Count = q14.Count,
                        Text = "累積注意未結案",
                        Status = "4"
                    });

                    //組織巡檢異常
                    var query2 = (from a in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join r in db.Route
                                  on c.RouteUniqueID equals r.UniqueID
                                  where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                  select new
                                  {
                                      c.EquipmentUniqueID,
                                      a.ClosedTime,
                                      c.CheckDate,
                                      c.IsAbnormal,
                                      c.IsAlert
                                  }).Distinct().ToList();

                    var q21 = query2.Where(x => x.CheckDate == today && x.IsAbnormal).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = q21.Count,
                        Text = "今日異常項目總數",
                        Status = "5"
                    });

                    var q22 = query2.Where(x => x.CheckDate == today && x.IsAlert && !q21.Contains(x.EquipmentUniqueID)).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = q22.Count,
                        Text = "今日注意項目總數",
                        Status = "6"
                    });

                    var q23 = query2.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = q23.Count,
                        Text = "累積異常未結案",
                        Status = "7"
                    });

                    var q24 = query2.Where(x => x.IsAlert && !q23.Contains(x.EquipmentUniqueID)).Select(x => x.EquipmentUniqueID).Distinct().ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-exclamation-circle",
                        Count = q24.Count,
                        Text = "累積注意未結案",
                        Status = "8"
                    });
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
    }
}
