using DbEntity.ASE;
using Models.ASE.QS.CheckItemRemarkManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QS
{
    public class CheckItemRemarkDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QS_CHECKITEM.ToList();

                    var queryResult = query.Select(x => new
                    {
                        UniqueID = x.UNIQUEID,
                        CheckTypeID = x.TYPEID.ToString(),
                        CheckTypeEDescription = x.TYPEEDESCRIPTION,
                        CheckTypeCDescription = x.TYPECDESCRIPTION,
                        CheckItemID = x.ID.ToString(),
                        CheckItemEDescription = x.EDESCRIPTION,
                        CheckItemCDescription = x.CDESCRIPTION
                    }).ToList();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        queryResult = queryResult.Where(x => x.CheckTypeID.Contains(Parameters.KeyWord) || x.CheckTypeEDescription.Contains(Parameters.KeyWord) || x.CheckTypeCDescription.Contains(Parameters.KeyWord) || x.CheckItemID.Contains(Parameters.KeyWord) || x.CheckItemEDescription.Contains(Parameters.KeyWord) || x.CheckItemCDescription.Contains(Parameters.KeyWord)).ToList();
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = queryResult.Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            CheckTypeID = decimal.Parse(x.CheckTypeID),
                            CheckTypeDescription = x.CheckTypeCDescription,
                            CheckItemID = decimal.Parse(x.CheckItemID),
                            CheckItemDescription = x.CheckItemCDescription
                        }).OrderBy(x => x.CheckTypeID).ThenBy(x => x.CheckItemID).ToList()
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = (from c in db.QS_CHECKITEM
                                     where c.UNIQUEID == UniqueID
                                     select new
                                     {
                                         UniqueID = c.UNIQUEID,
                                         CheckTypeID = c.TYPEID,
                                         CheckTypeEDescription = c.TYPEEDESCRIPTION,
                                         CheckTypeCDescription = c.TYPECDESCRIPTION,
                                         CheckItemID = c.ID,
                                         CheckItemEDescription = c.EDESCRIPTION,
                                         CheckItemCDescription = c.CDESCRIPTION,
                                         CheckTimes = c.CHECKTIMES,
                                         Unit = c.UNIT
                                     }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = checkItem.UniqueID,
                        CheckTypeID = checkItem.CheckTypeID,
                        CheckTypeCDescription = checkItem.CheckTypeCDescription,
                        CheckTypeEDescription = checkItem.CheckTypeEDescription,
                        CheckItemID = checkItem.CheckItemID,
                        CheckItemCDescription = checkItem.CheckItemCDescription,
                        CheckItemEDescription = checkItem.CheckItemEDescription,
                        CheckTimes = checkItem.CheckTimes,
                        Unit = checkItem.Unit,
                        RemarkList = (from x in db.QS_CHECKITEMREMARK
                                      join r in db.QS_REMARK
                                      on x.REMARKUNIQUEID equals r.UNIQUEID
                                      where x.CHECKITEMUNIQUEID == checkItem.UniqueID
                                      select r.DESCRIPTION).OrderBy(x => x).ToList()
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = (from c in db.QS_CHECKITEM
                                     where c.UNIQUEID == UniqueID
                                     select new
                                     {
                                         UniqueID = c.UNIQUEID,
                                         CheckTypeID = c.TYPEID,
                                         CheckTypeEDescription = c.TYPEEDESCRIPTION,
                                         CheckTypeCDescription = c.TYPECDESCRIPTION,
                                         CheckItemID = c.ID,
                                         CheckItemEDescription = c.EDESCRIPTION,
                                         CheckItemCDescription = c.CDESCRIPTION,
                                         CheckTimes = c.CHECKTIMES,
                                         Unit = c.UNIT
                                     }).First();

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = checkItem.UniqueID,
                        CheckTypeID = checkItem.CheckTypeID,
                        CheckTypeCDescription = checkItem.CheckTypeCDescription,
                        CheckTypeEDescription = checkItem.CheckTypeEDescription,
                        CheckItemID = checkItem.CheckItemID,
                        CheckItemCDescription = checkItem.CheckItemCDescription,
                        CheckItemEDescription = checkItem.CheckItemEDescription,
                        CheckTimes = checkItem.CheckTimes,
                        Unit = checkItem.Unit,
                        RemarkList = db.QS_REMARK.Select(x => new RemarkModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        CheckItemRemarkList = db.QS_CHECKITEMREMARK.Where(x => x.CHECKITEMUNIQUEID == checkItem.UniqueID).Select(x => x.REMARKUNIQUEID).ToList()
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    using (TransactionScope trans = new TransactionScope())
                    {
                        db.QS_CHECKITEMREMARK.RemoveRange(db.QS_CHECKITEMREMARK.Where(x => x.CHECKITEMUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();

                        db.QS_CHECKITEMREMARK.AddRange(Model.FormInput.RemarkList.Select(x => new QS_CHECKITEMREMARK
                        {
                            CHECKITEMUNIQUEID = Model.UniqueID,
                            REMARKUNIQUEID = x
                        }).ToList());

                        db.SaveChanges();

                        trans.Complete();
                    }

                    result.ReturnSuccessMessage("編輯稽核項目與備註內容綁定成功");
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
