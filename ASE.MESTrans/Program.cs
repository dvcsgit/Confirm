using DataAccess.ASE;
using DbEntity.ASE;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ASE.MESTrans
{
    class Program
    {
        //static void ImportFWCATNS_PMBUYOFFLIST(List<string> ExcelList)
        //{
        //    foreach (var excel in ExcelList)
        //    {
        //        ImportFWCATNS_PMBUYOFFLIST(excel);
        //    }
        //}

        //static void ImportFWCATNS_PMBUYOFFLIST(string Excel)
        //{
        //    var b = new XSSFWorkbook(Excel);

        //    var s = b.GetSheetAt(0);

        //    using (ASEDbEntities db = new ASEDbEntities())
        //    {
        //        for (int rowIndex = 1; rowIndex <= s.LastRowNum; rowIndex++)
        //        {
        //            db.FWCATNS_PMBUYOFFLIST.Add(new FWCATNS_PMBUYOFFLIST()
        //            {
        //                UNIQUEID = Guid.NewGuid().ToString(),
        //                MODEL = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(0)),
        //                PMTYPE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(1)),
        //                BUYOFFNO = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(2)),
        //                ITEM = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(3)),
        //                CRITERIA = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(4)),
        //                SAMPLESIZE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(5)),
        //                INITIALTIME = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(6)),
        //                UPDATETIME = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(7)),
        //                EDITOR = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(8)),
        //                ACTIVESTATUS = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(9)),
        //                AREA = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(10)),
        //                PLANT = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(11)),
        //                SEQNO = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(12)),
        //                UNIT = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(13)),
        //                AUTOMOTIVE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(14))
        //            });
        //        }

        //        db.SaveChanges();
        //    }
        //}

        //static void ImportFWCATNS_DOWNTIMEEQUIP(List<string> ExcelList)
        //{
        //    foreach (var excel in ExcelList)
        //    {
        //        ImportFWCATNS_DOWNTIMEEQUIP(excel);
        //    }
        //}

        //static void ImportFWCATNS_DOWNTIMEEQUIP(string Excel)
        //{
        //    var b = new XSSFWorkbook(Excel);

        //    var s = b.GetSheetAt(0);

        //    using (ASEDbEntities db = new ASEDbEntities())
        //    {
        //        for (int rowIndex = 1; rowIndex <= s.LastRowNum; rowIndex++)
        //        {
        //            db.FWCATNS_DOWNTIMEEQUIP.Add(new FWCATNS_DOWNTIMEEQUIP()
        //            {
        //                UNIQUEID = Guid.NewGuid().ToString(),
        //                EQPTID = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(0)),
        //                REGISTERTIME = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(1)),
        //                PLANT = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(2)),
        //                PRODAREA = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(3)),
        //                PRODLINE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(4)),
        //                STEPNAME = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(5)),
        //                MODEL = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(6)),
        //                OPERID = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(7)),
        //                SHIFT = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(8)),
        //                REASONCODE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(9)),
        //                STATUS = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(10)),
        //                LOCATION = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(11)),
        //                HANDLER = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(12)),
        //                LOTTYPE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(13)),
        //                PRIORITY = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(14)),
        //                PRODUCT_TYPE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(15)),
        //                VENDOR = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(16)),
        //                MC_STATUS = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(17)),
        //                ASSET_NO = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(18)),
        //                EQPSEQ = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(19)),
        //                CATEGORY = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(20)),
        //                OLPDEFECTCONTROL = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(21)),
        //                MARKOUTROTATE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(22)),
        //                SUM_DOWNRECORD = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(23)),
        //                EQPGAS = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(24)),
        //                DIEBONDSPC = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(25)),
        //                FEEDER = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(26)),
        //                MAGAZINE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(27)),
        //                VENDORINTIME = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(28)),
        //                EQPSTATE = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(29)),
        //                PRODUCTGROUP = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(30)),
        //                MAINTAINRECORD = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(31)),
        //                HDREVISENO = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(32)),
        //                HDNEWNO = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(33)),
        //                SWREVISENO = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(34)),
        //                SWNEWVERSION = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(35)),
        //                DIAMAFLOW = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(36)),
        //                EQPEFOGAS = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(37)),
        //                EQPNVTGAS = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(38)),
        //                DISPATCHSTEPNAME = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(39))
        //            });
        //        }

        //        db.SaveChanges();
        //    }
        //}

        //static void Import()
        //{
        //    var pmList = new List<string>() 
        //    { 
        //        @"C:\Users\N000114964\Desktop\ASE.MES\FWCATNS_PMCHECKLIST__FT.xlsx",
        //        @"C:\Users\N000114964\Desktop\ASE.MES\FWCATNS_PMCHECKLIST_ASSY.xlsx"
        //    };

        //    ImportFWCATNS_PMBUYOFFLIST(pmList);

        //    var equipmentList = new List<string>() 
        //    { 
        //        @"C:\Users\N000114964\Desktop\ASE.MES\FWCATNS_DOWNTIMEEQUIP_ASSY.xlsx",
        //        @"C:\Users\N000114964\Desktop\ASE.MES\FWCATNS_DOWNTIMEEQUIP_FT.xlsx"
        //    };

        //    ImportFWCATNS_DOWNTIMEEQUIP(equipmentList);
        //}

        public class TagData
        {
            public string ControlPoint { get; set; }

            public string TagID { get; set; }
        }

        static void Main(string[] args)
        {
            //var b = new XSSFWorkbook(@"C:\Users\eins\Desktop\TagRecover\tag.xlsx");

            //var s = b.GetSheetAt(0);

            //var tagDataList = new List<TagData>();

            //for (int rowIndex = 0; rowIndex <= s.LastRowNum; rowIndex++)
            //{
            //    tagDataList.Add(new TagData()
            //    {
            //        ControlPoint = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(0)),
            //        TagID = ExcelHelper.GetCellValue(s.GetRow(rowIndex).GetCell(1))
            //    });
            //}

            ////var total = 0;
            ////var notExist = 0;
            ////var tooMuch = 0;
            ////var ok = 0;

            //using (ASEDbEntities db = new ASEDbEntities())
            //{
            //    var query = db.Database.SqlQuery<CONTROLPOINT>("SELECT * FROM EIPC.CONTROLPOINT WHERE LENGTH(TAGID) = 14").ToList();

            //    //total = query.Count;

            //    foreach (var q in query)
            //    {
            //        var tmp = tagDataList.First(x => x.ControlPoint.Contains(q.ID));

            //        var c = db.CONTROLPOINT.First(x => x.UNIQUEID == q.UNIQUEID);

            //        c.TAGID = tmp.TagID;

            //        //var tmp = tagDataList.Where(x => x.ControlPoint.Contains(q.ID)).ToList();

            //        //if (tmp.Count > 0)
            //        //{
            //        //    if (tmp.Count > 1)
            //        //    {
            //        //        tooMuch++;
            //        //    }
            //        //    else
            //        //    {
            //        //        ok++;
            //        //    }
            //        //}
            //        //else
            //        //{
            //        //    notExist++;
            //        //}
            //    }

            //    db.SaveChanges();
            //}

            //Console.WriteLine(string.Format("Total:{0}", total));
            //Console.WriteLine(string.Format("OK:{0}", ok));
            //Console.WriteLine(string.Format("Too Much:{0}", tooMuch));
            //Console.WriteLine(string.Format("Not Exist:{0}", notExist));

            //Console.Read();
        }

        //static void Main(string[] args)
        //{
        //    //Import();

        //    using (ASEDbEntities db = new ASEDbEntities())
        //    {
        //        //var query = (from x in db.FWCATNS_DOWNTIMEEQUIP
        //        //             join o in db.ORGANIZATION
        //        //             on x.PRODLINE equals o.ID
        //        //             select o.UNIQUEID).Distinct().ToList();

        //        //foreach (var q in query)
        //        //{
        //            var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList("37e352c5-1c67-4ef2-af4e-7ade7946f5d5", false);

        //            foreach (var d in downStream)
        //            {
        //                db.QUERYABLEORGANIZATION.Add(new QUERYABLEORGANIZATION()
        //                {
        //                    ORGANIZATIONUNIQUEID = d,
        //                    QUERYABLEORGANIZATIONUNIQUEID = "37e352c5-1c67-4ef2-af4e-7ade7946f5d5"
        //                });
        //            }
        //        //}

        //        db.SaveChanges();
        //    }

        //    //using (ASEDbEntities db = new ASEDbEntities())
        //    //{
        //    //    var query = db.FWCATNS_PMBUYOFFLIST.ToList();

        //    //    foreach (var q in query)
        //    //    {
        //    //        var isFeelItem = false;

        //    //        if (q.SAMPLESIZE.Contains("(A)"))
        //    //        {
        //    //            isFeelItem = true;

        //    //            var tmp = q.SAMPLESIZE.Split(' ');

        //    //            var seq = 1;

        //    //            for (int i = 1; i < tmp.Length; i = i + 2)
        //    //            {
        //    //                db.STANDARDFEELOPTION.Add(new STANDARDFEELOPTION()
        //    //                {
        //    //                    STANDARDUNIQUEID = q.UNIQUEID,
        //    //                    UNIQUEID = Guid.NewGuid().ToString(),
        //    //                    DESCRIPTION = tmp[i],
        //    //                    ISABNORMAL = "N",
        //    //                    SEQ = seq
        //    //                });

        //    //                seq++;
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            isFeelItem = false;
        //    //        }

        //    //        db.STANDARD.Add(new STANDARD()
        //    //        {
        //    //            UNIQUEID = q.UNIQUEID,
        //    //            MAINTENANCETYPE = q.MODEL,
        //    //            ID = q.BUYOFFNO,
        //    //            DESCRIPTION = q.ITEM,
        //    //            ACCUMULATIONBASE = null,
        //    //            ISACCUMULATION = "N",
        //    //            ISFEELITEM = isFeelItem ? "Y" : "N",
        //    //            LASTMODIFYTIME = DateTime.Now,
        //    //            ORGANIZATIONUNIQUEID = "2a54f076-f14c-44fd-9f42-b202ac9206e0",
        //    //            REMARK = q.CRITERIA
        //    //        });
        //    //    }

        //    //    db.SaveChanges();
        //    //}
        //}
    }
}
