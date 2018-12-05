using DbEntity.MSSQL.EquipmentMaintenance;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace TAIWAY.FEM2ERP
{
    public class ImportItem
    {
        public string Cycle { get; set; }
        public string Organization { get; set; }
        public string EquipmentID { get; set; }
        public string EquipmentName { get; set; }
        public string MaintenanceType { get; set; }
        public string Standard { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (EDbEntities db = new EDbEntities())
            {
                var query = (from x in db.EquipmentStandard
                             join e in db.Equipment
                             on x.EquipmentUniqueID equals e.UniqueID
                             join s in db.Standard
                             on x.StandardUniqueID equals s.UniqueID
                             select new
                             {
                                 s.MaintenanceType,
                                 e.UniqueID,
                                 e.OrganizationUniqueID,
                                 e.ID,
                                 e.Name
                             }).Distinct().ToList();

                foreach (var q in query)
                {
                    var cycleType = q.MaintenanceType.Substring(q.MaintenanceType.LastIndexOf("-") + 1);

                    var jobUniqueID = Guid.NewGuid().ToString();

                    if (cycleType == "A")
                    {
                        db.MJob.Add(new MJob()
                        {
                            UniqueID = jobUniqueID,
                            Description = string.Format("{0}-{1}類保養", q.Name, cycleType),
                            BeginDate = new DateTime(2018, 1, 1),
                            CycleCount = 1,
                            CycleMode = "M",
                            EndDate = null,
                            LastModifyTime = DateTime.Now,
                            NotifyDay = 7,
                            OrganizationUniqueID = q.OrganizationUniqueID,
                            Remark = null
                        });
                    }
                    if (cycleType == "B")
                    {
                        db.MJob.Add(new MJob()
                        {
                            UniqueID = jobUniqueID,
                            Description = string.Format("{0}-{1}類保養", q.Name, cycleType),
                            BeginDate = new DateTime(2018, 1, 1),
                            CycleCount = 3,
                            CycleMode = "M",
                            EndDate = null,
                            LastModifyTime = DateTime.Now,
                            NotifyDay = 7,
                            OrganizationUniqueID = q.OrganizationUniqueID,
                            Remark = null
                        });
                    }
                    if (cycleType == "C")
                    {
                        db.MJob.Add(new MJob()
                        {
                            UniqueID = jobUniqueID,
                            Description = string.Format("{0}-{1}類保養", q.Name, cycleType),
                            BeginDate = new DateTime(2018, 1, 1),
                            CycleCount = 1,
                            CycleMode = "Y",
                            EndDate = null,
                            LastModifyTime = DateTime.Now,
                            NotifyDay = 7,
                            OrganizationUniqueID = q.OrganizationUniqueID,
                            Remark = null
                        });
                    }

                    db.MJobEquipment.Add(new MJobEquipment()
                    {
                        MJobUniqueID = jobUniqueID,
                        EquipmentUniqueID = q.UniqueID,
                        PartUniqueID = "*"
                    });

                    var standardList = (from x in db.EquipmentStandard
                                        join s in db.Standard
                                        on x.StandardUniqueID equals s.UniqueID
                                        where x.EquipmentUniqueID == q.UniqueID && s.MaintenanceType == q.MaintenanceType
                                        select s.UniqueID).ToList();

                    db.MJobEquipmentStandard.AddRange(standardList.Select(x => new MJobEquipmentStandard
                    {
                        MJobUniqueID = jobUniqueID,
                        EquipmentUniqueID = q.UniqueID,
                        PartUniqueID = "*",
                        StandardUniqueID = x
                    }).ToList());
                }

                db.SaveChanges();
            }
        }

        //static void Main(string[] args)
        //{
        //    var organizationDictionary = new Dictionary<string, string>() 
        //    { 
        //        { "#200","4e7b364f-7a98-4043-9e3b-9aac2d0bac47"},
        //        { "#300","84938f7c-6203-4488-aa43-f88577d7646b"},
        //        { "#400","735372a3-8ad7-4b97-a4bf-d53825de0a0d"},
        //        { "#500","711983bd-8b6b-4d78-97ea-2c4da5d9cf74"},
        //        { "#650","f9e771d5-89e2-482b-afc6-606d1d9e278c"},
        //        { "#700","170d5722-d9b2-4901-9b0b-25b86b331d60"},
        //        { "#850","6fe6604f-1073-428e-a55c-5ae2b4842ed0"}
        //    };

        //    var workBook = new XSSFWorkbook(@"C:\Users\ftc\Desktop\i.xlsx");

        //    var sheet = workBook.GetSheetAt(0);

        //    using (EDbEntities db = new EDbEntities())
        //    {
        //        var itemList = new List<ImportItem>();

        //        var prevOrganization = string.Empty;
        //        var prevEquipmentID = string.Empty;
        //        var prevEquipmentName = string.Empty;
        //        var prevMaintenanceType = string.Empty;
        //        var prevStandard = string.Empty;

        //        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        //        {
        //            var row = sheet.GetRow(rowIndex);

        //            var organization = ExcelHelper.GetCellValue(row.GetCell(1));

        //            if (string.IsNullOrEmpty(organization))
        //            {
        //                organization = prevOrganization;
        //            }

        //            var equipmentID = ExcelHelper.GetCellValue(row.GetCell(2));

        //            if (string.IsNullOrEmpty(equipmentID))
        //            {
        //                equipmentID = prevEquipmentID;
        //            }

        //            var equipmentName = ExcelHelper.GetCellValue(row.GetCell(3));

        //            if (string.IsNullOrEmpty(equipmentName))
        //            {
        //                equipmentName = prevEquipmentName;
        //            }

        //            var maintenanceType = ExcelHelper.GetCellValue(row.GetCell(4));

        //            if (string.IsNullOrEmpty(maintenanceType))
        //            {
        //                maintenanceType = prevMaintenanceType;
        //            }

        //            var standard = ExcelHelper.GetCellValue(row.GetCell(5));

        //            if (string.IsNullOrEmpty(standard))
        //            {
        //                standard = prevStandard;
        //            }

        //            prevOrganization = organization;
        //            prevEquipmentID = equipmentID;
        //            prevEquipmentName = equipmentName;
        //            prevMaintenanceType = maintenanceType;
        //            prevStandard = standard;

        //            itemList.Add(new ImportItem()
        //            {
        //                Cycle = ExcelHelper.GetCellValue(row.GetCell(0)),
        //                Organization = organization,
        //                EquipmentID = equipmentID,
        //                EquipmentName = equipmentName,
        //                MaintenanceType = maintenanceType,
        //                Standard = standard
        //            });
        //        }

        //        var equipmentList = itemList.Select(x => new { x.Organization, x.EquipmentID }).Distinct().ToList();

        //        foreach (var equipment in equipmentList)
        //        {
        //            var organizationUniqueID = organizationDictionary[equipment.Organization];

        //            var e = db.Equipment.FirstOrDefault(x => x.OrganizationUniqueID == organizationUniqueID && x.ID == equipment.EquipmentID);

        //            var equipmentName = itemList.First(x => x.Organization == equipment.Organization && x.EquipmentID == equipment.EquipmentID).EquipmentName;

        //            var equipmentUniqueID = Guid.NewGuid().ToString();

        //            if (e != null)
        //            {
        //                equipmentUniqueID = e.UniqueID;
        //                e.Name = equipmentName;
        //            }
        //            else
        //            {
        //                db.Equipment.Add(new Equipment()
        //                {
        //                    UniqueID = equipmentUniqueID,
        //                    ID = equipment.EquipmentID,
        //                    OrganizationUniqueID = organizationUniqueID,
        //                    MaintenanceOrganizationUniqueID = "b65d9b79-ce45-4c7b-8b7e-2f608c5f8b43",
        //                    IsFeelItemDefaultNormal = false,
        //                    LastModifyTime = DateTime.Now,
        //                    Name = equipmentName
        //                });
        //            }

        //            var maintenanceCycleList = itemList.Where(x => x.EquipmentID == equipment.EquipmentID).Select(x => x.Cycle).Distinct().ToList();

        //            foreach (var maintenanceCycle in maintenanceCycleList)
        //            {
        //                var query = itemList.Where(x => x.Cycle == maintenanceCycle && x.EquipmentID == equipment.EquipmentID).ToList();

        //                var seq = 1;

        //                foreach (var q in query)
        //                {
        //                    var uniqueID = Guid.NewGuid().ToString();

        //                    db.Standard.Add(new Standard()
        //                    {
        //                        UniqueID = uniqueID,
        //                        OrganizationUniqueID = organizationUniqueID,
        //                        MaintenanceType = string.Format("{0}-{1}", q.EquipmentID, q.Cycle),
        //                        ID = seq.ToString().PadLeft(2, '0'),
        //                        AccumulationBase = null,
        //                        Description = string.Format("【{0}】{1}", q.MaintenanceType, q.Standard),
        //                        IsAccumulation = false,
        //                        IsFeelItem = true,
        //                        LastModifyTime = DateTime.Now,
        //                        LowerAlertLimit = null,
        //                        LowerLimit = null,
        //                        Remark = null,
        //                        Unit = null,
        //                        UpperAlertLimit = null,
        //                        UpperLimit = null
        //                    });

        //                    db.StandardFeelOption.Add(new StandardFeelOption()
        //                    {
        //                        UniqueID = Guid.NewGuid().ToString(),
        //                        StandardUniqueID = uniqueID,
        //                        Seq = 1,
        //                        IsAbnormal = false,
        //                        Description = "正常"
        //                    });

        //                    db.StandardFeelOption.Add(new StandardFeelOption()
        //                    {
        //                        UniqueID = Guid.NewGuid().ToString(),
        //                        StandardUniqueID = uniqueID,
        //                        Seq = 2,
        //                        IsAbnormal = true,
        //                        Description = "異常"
        //                    });

        //                    db.EquipmentStandard.Add(new EquipmentStandard()
        //                    {
        //                        EquipmentUniqueID = equipmentUniqueID,
        //                        StandardUniqueID = uniqueID,
        //                        AccumulationBase = null,
        //                        IsInherit = true,
        //                        LowerAlertLimit = null,
        //                        LowerLimit = null,
        //                        PartUniqueID = "*",
        //                        Remark = null,
        //                        Unit = null,
        //                        UpperAlertLimit = null,
        //                        UpperLimit = null
        //                    });

        //                    seq++;
        //                }
        //            }
        //        }

        //        db.SaveChanges();
        //    }
        //}

        //static void Main(string[] args)
        //{
        //    var workBook = new XSSFWorkbook(@"C:\Users\ftc\Desktop\I.xlsx");

        //    var sheet = workBook.GetSheetAt(2);

        //    using (EDbEntities db = new EDbEntities())
        //    {
        //        var specList = db.MaterialSpec.ToList();

        //        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        //        {
        //            var row = sheet.GetRow(rowIndex);

        //            var location = ExcelHelper.GetCellValue(row.GetCell(0));
        //            var productid = ExcelHelper.GetCellValue(row.GetCell(1));
        //            var id = ExcelHelper.GetCellValue(row.GetCell(2));
        //            var type = ExcelHelper.GetCellValue(row.GetCell(5));
        //            var name = ExcelHelper.GetCellValue(row.GetCell(6));
        //            var brand = ExcelHelper.GetCellValue(row.GetCell(7));
        //            var qty = ExcelHelper.GetCellValue(row.GetCell(8));
        //            var safeqty = ExcelHelper.GetCellValue(row.GetCell(9));
        //            var remark = ExcelHelper.GetCellValue(row.GetCell(10));

        //            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
        //            {
        //                var uniqueID = Guid.NewGuid().ToString();

        //                int q = 0;

        //                if (!string.IsNullOrEmpty(qty))
        //                {
        //                    q = int.Parse(qty);
        //                }

        //                db.Material.Add(new Material()
        //                {
        //                    UniqueID = uniqueID,
        //                    OrganizationUniqueID = "4eebf30e-7b35-46a7-a64e-04b1719ee5c1",
        //                    ID = id,
        //                    EquipmentType = type,
        //                    Name = name,
        //                    Quantity = q
        //                });

        //                foreach (var spec in specList)
        //                {
        //                    if (spec.Description == "儲位")
        //                    {
        //                        db.MaterialSpecValue.Add(new MaterialSpecValue()
        //                        {
        //                            MaterialUniqueID = uniqueID,
        //                            SpecUniqueID = spec.UniqueID,
        //                            Seq = 1,
        //                            Value = location
        //                        });
        //                    }
        //                    if (spec.Description == "商別碼")
        //                    {
        //                        db.MaterialSpecValue.Add(new MaterialSpecValue()
        //                        {
        //                            MaterialUniqueID = uniqueID,
        //                            SpecUniqueID = spec.UniqueID,
        //                            Seq = 2,
        //                            Value = productid
        //                        });
        //                    }
        //                    if (spec.Description == "廠牌")
        //                    {
        //                        db.MaterialSpecValue.Add(new MaterialSpecValue()
        //                        {
        //                            MaterialUniqueID = uniqueID,
        //                            SpecUniqueID = spec.UniqueID,
        //                            Seq = 3,
        //                            Value = brand
        //                        });
        //                    }
        //                    if (spec.Description == "安全庫存量")
        //                    {
        //                        db.MaterialSpecValue.Add(new MaterialSpecValue()
        //                        {
        //                            MaterialUniqueID = uniqueID,
        //                            SpecUniqueID = spec.UniqueID,
        //                            Seq = 4,
        //                            Value = safeqty
        //                        });
        //                    }

        //                    if (spec.Description == "備註")
        //                    {
        //                        db.MaterialSpecValue.Add(new MaterialSpecValue()
        //                        {
        //                            MaterialUniqueID = uniqueID,
        //                            SpecUniqueID = spec.UniqueID,
        //                            Seq = 5,
        //                            Value = remark
        //                        });
        //                    }
        //                }
        //            }

        //        }

        //        db.SaveChanges();
        //    }
        //}
    }
}
