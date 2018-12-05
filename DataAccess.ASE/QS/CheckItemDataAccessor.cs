using DbEntity.ASE;
using Models.ASE.QS.CheckItemManagement;
using Utility.Models;
using System.Linq;
using System;
using System.Reflection;
using Utility;
using System.Transactions;

namespace DataAccess.ASE.QS
{
    public class CheckItemDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QS_CHECKITEM.Select(x => new
                    {
                        x.TYPEID,
                        x.TYPEEDESCRIPTION,
                        x.TYPECDESCRIPTION
                    }).Distinct().ToList();

                    var queryResult = query.Select(x => new
                    {
                        ID = x.TYPEID.ToString(),
                        EDescription = x.TYPEEDESCRIPTION,
                        CDescription = x.TYPECDESCRIPTION
                    }).ToList();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        queryResult = queryResult.Where(x => x.ID.Contains(Parameters.KeyWord) || x.EDescription.Contains(Parameters.KeyWord) || x.CDescription.Contains(Parameters.KeyWord)).ToList();
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = queryResult.Select(x => new GridItem()
                        {
                            ID = decimal.Parse(x.ID),
                            EDescription = x.EDescription,
                            CDescription = x.CDescription
                        }).OrderBy(x => x.ID).ToList()
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

        public static RequestResult GetDetailViewModel(decimal TypeID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItemList = db.QS_CHECKITEM.Where(x => x.TYPEID == TypeID).ToList();

                    result.ReturnData(new DetailViewModel()
                    {
                        ID = checkItemList.First().TYPEID,
                        CDescription = checkItemList.First().TYPECDESCRIPTION,
                        EDescription = checkItemList.First().TYPEEDESCRIPTION,
                        ItemList = checkItemList.Select(x => new CheckItemModel
                        {
                            UniqueID = x.UNIQUEID,
                            ID = x.ID,
                            CDescription = x.CDESCRIPTION,
                            EDescription = x.EDESCRIPTION,
                            CheckTimes = x.CHECKTIMES,
                            Unit = x.UNIT
                        }).OrderBy(x => x.ID).ToList()
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.QS_CHECKITEM.FirstOrDefault(x => x.TYPEID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        foreach (var checkItemString in Model.FormInput.CheckItemStringList)
                        {
                            string[] temp = checkItemString.Split(Define.Seperators, StringSplitOptions.None);

                            var id = temp[0];
                            var cDescription = temp[1];
                            var eDescription = temp[2];
                            var checkTimes = int.Parse(temp[3]);
                            var unit = temp[4];

                            db.QS_CHECKITEM.Add(new QS_CHECKITEM()
                            {
                                UNIQUEID = Guid.NewGuid().ToString(),
                                TYPEID = Model.FormInput.ID,
                                TYPEEDESCRIPTION = Model.FormInput.EDescription,
                                TYPECDESCRIPTION = Model.FormInput.CDescription,
                                ID = decimal.Parse(id),
                                EDESCRIPTION = eDescription,
                                CDESCRIPTION = cDescription,
                                CHECKTIMES = checkTimes,
                                UNIT = unit
                            });
                        }

                        db.SaveChanges();

                        result.ReturnData(Model.FormInput.ID, "新增稽核類別成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("稽核類別代號已存在");
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

        public static RequestResult GetEditFormModel(decimal TypeID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItemList = db.QS_CHECKITEM.Where(x => x.TYPEID == TypeID).ToList();

                    result.ReturnData(new EditFormModel()
                    {
                        FormInput = new FormInput()
                        {
                            ID = checkItemList.First().TYPEID,
                            EDescription = checkItemList.First().TYPEEDESCRIPTION,
                            CDescription = checkItemList.First().TYPECDESCRIPTION
                        },
                        ItemList = checkItemList.Select(x => new CheckItemModel
                        {
                            UniqueID = x.UNIQUEID,
                            ID = x.ID,
                            EDescription = x.EDESCRIPTION,
                            CDescription = x.CDESCRIPTION,
                            CheckTimes = x.CHECKTIMES,
                            Unit = x.UNIT
                        }).OrderBy(x => x.ID).ToList()
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
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    db.QS_CHECKITEM.RemoveRange(db.QS_CHECKITEM.Where(x => x.TYPEID == Model.FormInput.ID).ToList());

                    db.SaveChanges();

                    foreach (var checkItemString in Model.FormInput.CheckItemStringList)
                    {
                        string[] temp = checkItemString.Split(Define.Seperators, StringSplitOptions.None);

                        var checkItemUniqueID = temp[0];
                        var id = decimal.Parse(temp[1]);
                        var cDescription = temp[2];
                        var eDescription = temp[3];
                        var checkTimes = int.Parse(temp[4]);
                        var unit = temp[5];

                        db.QS_CHECKITEM.Add(new QS_CHECKITEM()
                        {
                            UNIQUEID = !string.IsNullOrEmpty(checkItemUniqueID) ? checkItemUniqueID : Guid.NewGuid().ToString(),
                            TYPEID = Model.FormInput.ID,
                            TYPEEDESCRIPTION = Model.FormInput.EDescription,
                            TYPECDESCRIPTION = Model.FormInput.CDescription,
                            ID = id,
                            EDESCRIPTION = eDescription,
                            CDESCRIPTION = cDescription,
                            CHECKTIMES = checkTimes,
                            UNIT = unit
                        });
                    }

                    db.SaveChanges();

                    var checkItemList = db.QS_CHECKITEM.Select(x => x.UNIQUEID).ToList();

                    db.QS_CHECKITEMREMARK.RemoveRange(db.QS_CHECKITEMREMARK.Where(x => !checkItemList.Contains(x.CHECKITEMUNIQUEID)).ToList());

                    //db.Database.ExecuteSqlCommand("DELETE FROM EIPC.QS_CHECKITEMREMARK WHERE CHECKITEMUNIQUEID NOT IN (SELECT UNIQUEID FROM EIPC.QS_CHECKITEM)");

                    db.SaveChanges();

                    result.ReturnSuccessMessage("編輯稽核類別成功");

#if !DEBUG
                    trans.Complete();
                    }
#endif
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

        public static RequestResult Delete(decimal TypeID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItemList = db.QS_CHECKITEM.Where(x => x.TYPEID == TypeID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        db.QS_CHECKITEM.Remove(checkItem);
                        db.QS_CHECKITEMREMARK.RemoveRange(db.QS_CHECKITEMREMARK.Where(x => x.CHECKITEMUNIQUEID == checkItem.UNIQUEID).ToList());
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("刪除稽核類別成功");
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
