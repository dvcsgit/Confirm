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
using System.Threading.Tasks;

namespace OracleScriptsGenerator
{
    class Program
    {
        static StringBuilder scripts = new StringBuilder();

        static void Main(string[] args)
        {
            using (DbEntities db = new DbEntities())
            {
                ObjectContext objContext = ((IObjectContextAdapter)db).ObjectContext;

                Generate(objContext, db, "FEM");
            }

            using (EDbEntities db = new EDbEntities())
            {
                ObjectContext objContext = ((IObjectContextAdapter)db).ObjectContext;

                Generate(objContext, db, "FEM_E");
            }

            Console.WriteLine("Finished");

            Console.ReadLine();
        }

        static void Generate(ObjectContext ObjectContext, DbContext DbContext,string DBName)
        {
            scripts = new StringBuilder();

            MetadataWorkspace workspace = ObjectContext.MetadataWorkspace;

            IEnumerable<EntityType> tables = workspace.GetItems<EntityType>(DataSpace.SSpace);

            foreach (var table in tables)
            {
                scripts.Append(string.Format("CREATE TABLE {0} (", table.Name));

                foreach (var column in table.DeclaredMembers)
                {
                    var columnName = column.Name;

                    if (columnName.Length >= 30)
                    {
                        if (columnName == "MaintenanceOrganizationUniqueID")
                        {
                            columnName = "PMOrganizationUniqueID";
                        }
                        else if (columnName == "TimeSpanAbnormalReasonDescription")
                        {
                            columnName = "TimeSpanAbnormalReasonDesc";
                        }
                        else if (columnName == "PipelinePatrolCheckTypeUniqueID")
                        {
                            columnName = "PipePatrolCheckTypeUniqueID";
                        }
                        else if (columnName == "TimeSpanAbnormalReasonUniqueID")
                        {
                            columnName = "TSAbnormalReasonUniqueID";
                        }
                        else
                        {
                            var temp = "";
                        }
                    }

                    var type = column.TypeUsage.EdmType.Name;

                    if (type == "varchar")
                    {
                        var len = table.Properties[column.Name].MaxLength;

                        type = string.Format("VARCHAR2({0})", len);
                    }
                    else if (type == "nvarchar")
                    {
                        var len = table.Properties[column.Name].MaxLength*1.5;

                        type = string.Format("VARCHAR2({0})", len);
                    }
                    else if (type == "nvarchar(max)")
                    {
                        type = string.Format("VARCHAR2({0})", 2048);
                    }
                    else if (type == "date" || type == "datetime")
                    {
                        type = "DATE";
                    }
                    else if (type == "int")
                    {
                        type = "NUMBER(10)";
                    }
                    else if (type == "float")
                    {
                        if (column.Name == "LNG" || column.Name == "LAT")
                        {
                            type = "NUMBER(15,12)";
                        }
                        else if (column.Name == "MinTimeSpan" || column.Name == "TotalTimeSpan" || column.Name == "LowerLimit" || column.Name == "LowerAlertLimit" || column.Name == "UpperLimit" || column.Name == "UpperAlertLimit" || column.Name == "AccumulationBase" || column.Name == "FrontWorkingHour" || column.Name == "WorkingHour" || column.Name == "Value" || column.Name == "NetValue")
                        {
                            type = "NUMBER(10,2)";
                        }
                        else
                        {
                            var temp = "";
                        }
                    }
                    else if (type == "bit")
                    {
                        type = "VARCHAR2(1)";
                    }
                    else
                    {
                        var temp = "";
                    }
                   

                    scripts.Append(string.Format("{0} {1}, ", columnName, type));
                }

                var pk = string.Format("pk_{0}", table.Name);

                if (pk.Length > 30)
                {
                    if (pk == "pk_AuthGroupWebPermissionFunction")
                    {
                        pk = "pk_AuthGroupWebPermissionFunc";
                    }
                    else if (pk == "pk_AbnormalReasonHandlingMethod")
                    {
                        pk = "pk_AbnormalReasonHMethod";
                    }
                    else
                    {
                        pk = pk.Substring(0, 30);
                    }
                }

                scripts.Append(string.Format("Constraint {0} Primary key(", pk));

                foreach (var key in table.KeyMembers)
                {
                    scripts.Append(key.Name);

                    if (table.KeyMembers.IndexOf(key) == table.KeyMembers.Count - 1)
                    { }
                    else
                    {
                        scripts.Append(",");
                    }
                }

                scripts.AppendLine(") );");
            }

            System.IO.File.WriteAllText(string.Format("Scripts.{0}.txt", DBName), scripts.ToString());
        }
    }
}
