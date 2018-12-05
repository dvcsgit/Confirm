using DbEntity.ASE;
using Models.ASE.QA.DataSync;
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
    public class AbnormalFormHelper
    {
        public static RequestResult Create(AbnormalFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var calibrationForm = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == Model.CFormUniqueID);

                    if (calibrationForm.STATUS != "1")
                    {
                        result.ReturnFailedMessage("此單非執行中狀態，無法開立異常通知");
                    }
                    else
                    {
                        calibrationForm.STATUS = "7";

                        var vhnoPrefix = string.Format("A{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var vhnoSeq = 1;

                        var query = db.QA_ABNORMALFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                        if (query.Count > 0)
                        {
                            vhnoSeq = int.Parse(query.First().VHNO.Substring(7)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(3, '0'));

                        var formUniqueID = Guid.NewGuid().ToString();

                        db.QA_ABNORMALFORM.Add(new QA_ABNORMALFORM()
                        {
                            UNIQUEID = formUniqueID,
                            VHNO = vhno,
                            CALFORMUNIQUEID = calibrationForm.UNIQUEID,
                            CREATETIME = DateTime.Now,
                            STATUS = "1",
                            REMARK = Model.Remark
                        });

                        int seq = 1;

                        foreach (var item in Model.ItemList)
                        {
                            var tmp = item.CheckItemUniqueID.Split('_');

                            var checkItemSeq = int.Parse(tmp[1]);

                            var detail = db.QA_CALIBRATIONFORMDETAIL.First(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID && x.SEQ == checkItemSeq);

                            db.QA_ABNORMALFORMDETAIL.Add(new QA_ABNORMALFORMDETAIL()
                            {
                                FORMUNIQUEID = formUniqueID,
                                SEQ = seq,
                                CHARACTERISTIC = detail.CHARACTERISTIC,
                                READINGVALUE = item.Value,
                                STANDARD = item.Standard.ToString(),
                                TOLERANCE = detail.TOLERANCE.ToString(),
                                CALDATE = DateTime.Today,
                                CALIBRATIONPOINT = detail.CALIBRATIONPOINT,
                                USINGRANGE = detail.USINGRANGE
                            });

                            seq++;
                        }

                        foreach (var stduse in Model.STDUSEList)
                        {
                            var equipment = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == stduse);

                            if (equipment != null)
                            {
                                db.QA_ABNORMALFORMSTDUSE.Add(new QA_ABNORMALFORMSTDUSE
                                {
                                    FORMUNIQUEID = formUniqueID,
                                    EQUIPMENTUNIQUEID = equipment.UNIQUEID
                                });
                            }
                        }

                        db.SaveChanges();

                        result.Success();
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
    }
}
