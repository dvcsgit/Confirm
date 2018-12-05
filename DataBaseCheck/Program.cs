using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataBaseCheck
{
    class Program
    {
        static StringBuilder log = new StringBuilder();

        //static void Main(string[] args)
        //{
        //    using (DbEntities db = new DbEntities())
        //    {
        //        ObjectContext objContext = ((IObjectContextAdapter)db).ObjectContext;

        //        Check(objContext, db);
        //    }

        //    using (EDbEntities db = new EDbEntities())
        //    {
        //        ObjectContext objContext = ((IObjectContextAdapter)db).ObjectContext;

        //        Check(objContext, db);
        //    }

        //    System.IO.File.WriteAllText(string.Format("DataBaseCheckResult.{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss")), log.ToString());
        //}

        //static void Check(ObjectContext ObjectContext, DbContext DbContext)
        //{
        //    MetadataWorkspace workspace = ObjectContext.MetadataWorkspace;

        //    IEnumerable<EntityType> tables = workspace.GetItems<EntityType>(DataSpace.SSpace);

        //    foreach (var table in tables)
        //    {
        //        var errorList = new List<string>();

        //        foreach (var column in table.DeclaredMembers)
        //        {
        //            string sql = string.Format("SELECT {0} FROM [{1}]", column.Name, table.Name);

        //            try
        //            {
        //                var type = column.TypeUsage.EdmType.BaseType.Name;

        //                if (type == "String")
        //                {
        //                    var result = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
        //                }
        //                else if (type == "DateTime")
        //                {
        //                    var result = DbContext.Database.SqlQuery<DateTime?>(sql).FirstOrDefault();
        //                }
        //                else if (type == "Int32")
        //                {
        //                    var result = DbContext.Database.SqlQuery<int?>(sql).FirstOrDefault();
        //                }
        //                else if (type == "Boolean")
        //                {
        //                    var result = DbContext.Database.SqlQuery<Boolean>(sql).FirstOrDefault();
        //                }
        //                else if (type == "Double")
        //                {
        //                    var result = DbContext.Database.SqlQuery<Double?>(sql).FirstOrDefault();
        //                }
        //                else
        //                {
        //                    errorList.Add(column.Name);
        //                }
        //            }
        //            catch
        //            {
        //                errorList.Add(column.Name);
        //            }
        //        }

        //        if (errorList.Count > 0)
        //        {
        //            log.AppendLine("==================================================");

        //            log.AppendLine(string.Format("Table {0} Validate Failed", table.Name));

        //            log.AppendLine("--------------------------------------------------");

        //            foreach (var err in errorList)
        //            {    
        //                log.AppendLine(string.Format("Column {0} Validate Failed", err));
        //            }

        //            log.AppendLine("==================================================");
        //        }
        //        else
        //        {
        //            log.AppendLine(string.Format("Table {0} Validate Success", table.Name));
        //        }
        //    }
        //}

        static void Main(string[] args)
        {
            using (DbEntities db = new DbEntities())
            {
                ObjectContext objContext = ((IObjectContextAdapter)db).ObjectContext;

                Check(objContext, db);
            }

            using (EDbEntities db = new EDbEntities())
            {
                ObjectContext objContext = ((IObjectContextAdapter)db).ObjectContext;

                Check(objContext, db);
            }

            System.IO.File.WriteAllText(string.Format("DataBaseCheckResult.{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss")), log.ToString());
        }

        static void Check(ObjectContext ObjectContext, DbContext DbContext)
        {
            MetadataWorkspace workspace = ObjectContext.MetadataWorkspace;

            IEnumerable<EntityType> tables = workspace.GetItems<EntityType>(DataSpace.SSpace);

            foreach (var table in tables)
            {
                if (table.DeclaredMembers.Any(x => x.Name == "OrganizationUniqueID"))
                {
                    log.AppendLine(string.Format("UPDATE {0} SET OrganizationUniqueID = 'NOrganizationUniqueID' WHERE OrganizationUniqueID = 'OOrganizationUniqeuID'", table.Name));
                }
            }
        }
    }
}
