using DbEntity.MSSQL.EquipmentMaintenance;
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

namespace DataAccess.EquipmentMaintenance
{
    public class EquipmentResumeDataAccessor
    {
        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.OrganizationUniqueID),
                        ID = equipment.ID,
                        Name = equipment.Name,
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.MaintenanceOrganizationUniqueID),
                        SpecList = (from x in db.EquipmentSpecValue
                                    join s in db.EquipmentSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.EquipmentUniqueID == equipment.UniqueID
                                    select new SpecModel
                                    {
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o => new Models.EquipmentMaintenance.EquipmentResumeQuery.EquipmentSpecOption
                                                    {
                                                        UniqueID = o.UniqueID,
                                                        Description = o.Description,
                                                        SpecUniqueID = o.SpecUniqueID,
                                                        Seq = o.Seq
                                                    }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Description).ToList(),
                        MaterialList = (from x in db.EquipmentMaterial
                                        join m in db.Material
                                        on x.MaterialUniqueID equals m.UniqueID
                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                        select new MaterialModel
                                        {
                                            ID = m.ID,
                                            Name = m.Name,
                                            Quantity = x.Quantity
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = p.UniqueID,
                                        Description = p.Description,
                                        MaterialList = (from x in db.EquipmentMaterial
                                                        join m in db.Material
                                                        on x.MaterialUniqueID equals m.UniqueID
                                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                        select new MaterialModel
                                                        {
                                                            ID = m.ID,
                                                            Name = m.Name,
                                                            Quantity = x.Quantity
                                                        }).OrderBy(x => x.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList()
                    };

                    var repairFormList = db.RForm.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList();

                    foreach (var repairForm in repairFormList)
                    {
                        var partDescription = "*";

                        if (repairForm.PartUniqueID != "*")
                        {
                            var part = db.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == repairForm.PartUniqueID);

                            if (part != null)
                            {
                                partDescription = part.Description;
                            }
                        }

                        model.RecordList.Add(new RecordModel()
                        {
                            TypeID = "R",
                            Type = Resources.Resource.Repair,
                            PartDescription = partDescription,
                            Date = DateTimeHelper.DateTime2DateStringWithSeperator(repairForm.CreateTime),
                            VHNO = repairForm.VHNO,
                            Subject = repairForm.Subject
                        });
                    }

                    var checkResultList = db.CheckResult.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList();

                    var byDateList1 = checkResultList.Select(x => new {x.PartUniqueID, x.PartDescription, x.CheckDate }).Distinct().ToList();

                    foreach (var item in byDateList1)
                    {
                        var subject = Resources.Resource.Normal;

                        var abnormalList = checkResultList.Where(x =>x.CheckDate==item.CheckDate&& x.PartUniqueID == item.PartUniqueID&&(x.IsAbnormal||x.IsAlert)).ToList();

                        if (abnormalList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormal in abnormalList)
                            {
                                sb.Append(abnormal.CheckItemDescription);

                                if (abnormal.IsAbnormal)
                                {
                                    sb.Append("【"+Resources.Resource.Abnormal+"】");
                                }

                                if (abnormal.IsAbnormal)
                                {
                                    sb.Append("【" + Resources.Resource.Warning + "】");
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);
                        }

                        model.RecordList.Add(new RecordModel()
                        {
                            TypeID="P",
                            Type = Resources.Resource.Patrol,
                            PartDescription = item.PartDescription,
                            Date = DateTimeHelper.DateString2DateStringWithSeparator(item.CheckDate),
                            VHNO = string.Empty,
                            Subject = subject
                        });
                    }



                    var standardResultList = (from r in db.MFormStandardResult
                                              join s in db.MFormResult
                                              on r.ResultUniqueID equals s.UniqueID
                                              where r.EquipmentUniqueID == equipment.UniqueID
                                              select new {
                                                  r.PartUniqueID,
                                                  r.PartDescription,
                                                  r.IsAbnormal,
                                                  r.IsAlert,
                                                  r.StandardDescription,
                                              s.PMDate
                                              }).ToList();

                    var byDateList2 = standardResultList.Select(x => new { x.PartUniqueID, x.PartDescription, x.PMDate }).Distinct().ToList();

                    foreach (var item in byDateList2)
                    {
                        var subject = Resources.Resource.Normal;

                        var abnormalList = standardResultList.Where(x => x.PMDate == item.PMDate && x.PartUniqueID == item.PartUniqueID && (x.IsAbnormal || x.IsAlert)).ToList();

                        if (abnormalList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormal in abnormalList)
                            {
                                sb.Append(abnormal.StandardDescription);

                                if (abnormal.IsAbnormal)
                                {
                                    sb.Append("【" + Resources.Resource.Abnormal + "】");
                                }

                                if (abnormal.IsAbnormal)
                                {
                                    sb.Append("【" + Resources.Resource.Warning + "】");
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);
                        }

                        model.RecordList.Add(new RecordModel()
                        {
                            TypeID="M",
                            Type = Resources.Resource.Maintenance,
                            PartDescription = item.PartDescription,
                            Date = DateTimeHelper.DateString2DateStringWithSeparator(item.PMDate),
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
