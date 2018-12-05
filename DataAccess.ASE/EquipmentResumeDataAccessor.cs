using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentResumeQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
{
    public class EquipmentResumeDataAccessor
    {
        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        ID = equipment.ID,
                        Name = equipment.NAME,
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.PMORGANIZATIONUNIQUEID),
                        SpecList = (from x in db.EQUIPMENTSPECVALUE
                                    join s in db.EQUIPMENTSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new SpecModel
                                    {
                                        Description = s.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new Models.EquipmentMaintenance.EquipmentResumeQuery.EquipmentSpecOption
                                        {
                                            UniqueID = o.UNIQUEID,
                                            Description = o.DESCRIPTION,
                                            SpecUniqueID = o.SPECUNIQUEID,
                                            Seq = o.SEQ.Value
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Description).ToList(),
                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                        join m in db.MATERIAL
                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                        select new MaterialModel
                                        {
                                            ID = m.ID,
                                            Name = m.NAME,
                                            Quantity = x.QUANTITY.HasValue?x.QUANTITY.Value:0
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EQUIPMENTPART
                                    where p.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new PartModel
                                    {
                                        UniqueID = p.UNIQUEID,
                                        Description = p.DESCRIPTION,
                                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                                        join m in db.MATERIAL
                                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == p.UNIQUEID
                                                        select new MaterialModel
                                                        {
                                                            ID = m.ID,
                                                            Name = m.NAME,
                                                            Quantity = x.QUANTITY.HasValue ? x.QUANTITY.Value : 0
                                                        }).OrderBy(x => x.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList()
                    };

                    var repairFormList = db.RFORM.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList();

                    foreach (var repairForm in repairFormList)
                    {
                        var partDescription = "*";

                        if (repairForm.PARTUNIQUEID != "*")
                        {
                            var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == repairForm.PARTUNIQUEID);

                            if (part != null)
                            {
                                partDescription = part.DESCRIPTION;
                            }
                        }

                        model.RecordList.Add(new RecordModel()
                        {
                            TypeID = "R",
                            Type = Resources.Resource.Repair,
                            PartDescription = partDescription,
                            Date = DateTimeHelper.DateTime2DateStringWithSeperator(repairForm.CREATETIME),
                            VHNO = repairForm.VHNO,
                            Subject = repairForm.SUBJECT
                        });
                    }

                    var checkResultList = db.CHECKRESULT.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList();

                    var byDateList1 = checkResultList.Select(x => new { x.PARTUNIQUEID, x.PARTDESCRIPTION, x.CHECKDATE }).Distinct().ToList();

                    foreach (var item in byDateList1)
                    {
                        var subject = Resources.Resource.Normal;

                        var abnormalList = checkResultList.Where(x => x.CHECKDATE == item.CHECKDATE && x.PARTUNIQUEID == item.PARTUNIQUEID && (x.ISABNORMAL=="Y" || x.ISALERT=="Y")).ToList();

                        if (abnormalList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormal in abnormalList)
                            {
                                sb.Append(abnormal.CHECKITEMDESCRIPTION);

                                if (abnormal.ISABNORMAL=="Y")
                                {
                                    sb.Append("【" + Resources.Resource.Abnormal + "】");
                                }

                                if (abnormal.ISALERT=="Y")
                                {
                                    sb.Append("【" + Resources.Resource.Warning + "】");
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);
                        }

                        model.RecordList.Add(new RecordModel()
                        {
                            TypeID = "P",
                            Type = Resources.Resource.Patrol,
                            PartDescription = item.PARTDESCRIPTION,
                            Date = DateTimeHelper.DateString2DateStringWithSeparator(item.CHECKDATE),
                            VHNO = string.Empty,
                            Subject = subject
                        });
                    }



                    var standardResultList = (from r in db.MFORMSTANDARDRESULT
                                              join s in db.MFORMRESULT
                                              on r.RESULTUNIQUEID equals s.UNIQUEID
                                              where r.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                              select new
                                              {
                                                  r.PARTUNIQUEID,
                                                  r.PARTDESCRIPTION,
                                                  r.ISABNORMAL,
                                                  r.ISALERT,
                                                  r.STANDARDDESCRIPTION,
                                                  s.PMDATE
                                              }).ToList();

                    var byDateList2 = standardResultList.Select(x => new { x.PARTUNIQUEID, x.PARTDESCRIPTION, x.PMDATE }).Distinct().ToList();

                    foreach (var item in byDateList2)
                    {
                        var subject = Resources.Resource.Normal;

                        var abnormalList = standardResultList.Where(x => x.PMDATE == item.PMDATE && x.PARTUNIQUEID == item.PARTUNIQUEID && (x.ISABNORMAL == "Y" || x.ISALERT == "Y")).ToList();

                        if (abnormalList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormal in abnormalList)
                            {
                                sb.Append(abnormal.STANDARDDESCRIPTION);

                                if (abnormal.ISABNORMAL=="Y")
                                {
                                    sb.Append("【" + Resources.Resource.Abnormal + "】");
                                }

                                if (abnormal.ISALERT=="Y")
                                {
                                    sb.Append("【" + Resources.Resource.Warning + "】");
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);
                        }

                        model.RecordList.Add(new RecordModel()
                        {
                            TypeID = "M",
                            Type = Resources.Resource.Maintenance,
                            PartDescription = item.PARTDESCRIPTION,
                            Date = DateTimeHelper.DateString2DateStringWithSeparator(item.PMDATE),
                            VHNO = string.Empty,
                            Subject = subject
                        });
                    }

                    model.RecordList = model.RecordList.OrderBy(x => x.Date).ToList();

                    result.ReturnData(model);
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
