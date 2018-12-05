using Customized.FPTC.DbEntity;
using Customized.FPTC.Models.DispatchQuery;
using DbEntity.MSSQL.TruckPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace Customized.FPTC.DataAccess
{
    public class DispatchQueryHelper
    {
        public static RequestResult GetQueryFormModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new QueryFormModel()
                {
                    Parameters = new QueryParameters()
                    {
                        BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                        EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                    }
                };

                foreach (var company in Customized.FPTC.Utility.Config.CompanyList)
                {
                    model.CompanySelectItemList.Add(new SelectListItem()
                    {
                        Text = string.Format("{0}/{1}", company.ID, company.Name),
                        Value = company.ID
                    });
                }

                model.DepartmentList = Customized.FPTC.Utility.Config.DepartmentList;

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

        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<GridItem>();

                using (FPTCDbEntities db = new FPTCDbEntities())
                {
                    var q1 = db.veDispatchToEFPG.AsQueryable();

                    if (Parameters.BeginDate.HasValue)
                    {
                        q1 = q1.Where(x => x.disp_dt >= Parameters.BeginDate.Value);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        q1 = q1.Where(x => x.disp_dt <= Parameters.EndDate.Value);
                    }

                    if (!string.IsNullOrEmpty(Parameters.CarNo))
                    {
                        q1 = q1.Where(x => x.car_licence.Contains(Parameters.CarNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.CompanyID))
                    {
                        q1 = q1.Where(x => x.corp == Parameters.CompanyID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.DepartmentID))
                    {
                        q1 = q1.Where(x => x.dept == Parameters.DepartmentID);
                    }

                    itemList = q1.ToList().Select(x => new GridItem
                    {
                        CompanyID = x.corp,
                        CompanyName = x.corp_name,
                        DepartmentID = x.dept,
                        DepartmentName = x.dept_name,
                        CarNo = x.car_licence,
                        Driver = x.emp_name,
                        DispatchTime = x.disp_dt
                    }).OrderBy(x => x.DispatchTime).ThenBy(x => x.CompanyID).ThenBy(x => x.DepartmentID).ThenBy(x => x.CarNo).ToList();
                }

                using (TDbEntities db = new TDbEntities())
                {
                    var q2 = db.ArriveRecord.AsQueryable();

                    if(Parameters.BeginDate.HasValue)
                    {
                        var begin = DateTimeHelper.DateTime2DateString(Parameters.BeginDate);

                        q2 = q2.Where(x=>string.Compare(x.ArriveDate, begin) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        var end = DateTimeHelper.DateTime2DateString(Parameters.EndDate.Value.AddDays(-1));

                        q2 = q2.Where(x => string.Compare(x.ArriveDate, end) <= 0);
                    }

                    var truckBindingUniqueIDList = q2.Select(x => x.TruckBindingUniqueID).Distinct().ToList();

                    var truckBindingResultList = db.TruckBindingResult.Where(x => truckBindingUniqueIDList.Contains(x.UniqueID)).ToList();

                    foreach (var item in itemList)
                    {
                        var truckBindingResult = truckBindingResultList.FirstOrDefault(x => x.FirstTruckNo == item.CarNo && x.CheckDate == item.DispatchDate);

                        if (truckBindingResult != null)
                        {
                            item.CheckUser = truckBindingResult.CheckUser;
                            item.CompleteRate = truckBindingResult.CompleteRate;
                            item.HaveAbnormal = truckBindingResult.IsAbnormal;
                            item.HaveAlert = truckBindingResult.IsAlert;
                            item.LabelClass = truckBindingResult.CompleteRateLabelClass;
                            item.SecondTruckNo = truckBindingResult.SecondTruckNo;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Parameters.IsChecked))
                {
                    if (Parameters.IsChecked == "Y")
                    {
                        itemList = itemList.Where(x => x.IsChecked).ToList();
                    }

                    if (Parameters.IsChecked == "N")
                    {
                        itemList = itemList.Where(x => !x.IsChecked).ToList();
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

        public static RequestResult Export(List<GridItem> ItemList, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("派車與檢查紀錄比對查詢({0})", DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
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
