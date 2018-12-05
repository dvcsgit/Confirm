using CHIMEI.AIMS2FEM.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility;
using Utility.Models;

namespace CHIMEI.AIMS2FEM
{
    public class TransHelper : IDisposable
    {
        private const string ConfigFileName = "AIMS.xml";

        private static string ConfigFile
        {
            get
            {
                string exePath = System.AppDomain.CurrentDomain.BaseDirectory;

                string filePath = Path.Combine(exePath, ConfigFileName);

                if (System.IO.File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = Path.Combine(exePath, "bin", ConfigFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        return ConfigFileName;
                    }
                }
            }
        }

        private string ConnString
        {
            get
            {
                var config = XDocument.Load(ConfigFile);

               var host= config.Root.Element("DataBase").Attribute("Host").Value;
               var port = config.Root.Element("DataBase").Attribute("Port").Value;
               var serviceName = config.Root.Element("DataBase").Attribute("ServiceName").Value;
               var userID = config.Root.Element("DataBase").Attribute("UserID").Value;
               var pwd = config.Root.Element("DataBase").Attribute("Password").Value;

                OracleConnectionStringBuilder oraConnBuilder = new OracleConnectionStringBuilder();

                oraConnBuilder.DataSource = string.Format("(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVICE_NAME={2})))", host, port, serviceName);
                oraConnBuilder.PersistSecurityInfo = true;
                oraConnBuilder.UserID = userID;
                oraConnBuilder.Password = pwd;

                return oraConnBuilder.ToString();
            }
        }

        private List<MailAddress> ToList
        {
            get
            {
                var toList = new List<MailAddress>();

                try
                {
                    var config = XDocument.Load(ConfigFile);

                    var elementList = config.Root.Elements("MAIL").ToList();

                    foreach (var element in elementList)
                    {
                        toList.Add(new MailAddress(element.Attribute("EMAIL").Value, element.Attribute("NAME").Value));
                    }
                }
                catch (Exception ex)
                {
                    toList = new List<MailAddress>();

                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                }

                return toList;
            }
        }

        public void Trans()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnString))
                {
                    conn.Open();

                    TransHROrganization(conn);
                    TransHRAccount(conn);
                    TransOrganization(conn);
                    TransEquipment(conn);
                    TransDeletedEquipment(conn);
                    TransJob(conn);

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void TransHROrganization(OracleConnection Conn)
        {
            try
            {
                using (OracleCommand cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CHIMEI.HR_ORGANIZATION";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            adapter.Fill(dt);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                var organizationList = dt.AsEnumerable().Select(x => new HR_Organization
                                {
                                    ID = x["ID"].ToString(),
                                    Name = x["NAME"].ToString(),
                                    ParentID = x["PARENTID"].ToString()
                                }).ToList();

                                using (DbEntities db = new DbEntities())
                                {
                                    TransHROrganization("*", "*", organizationList, db);

                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void TransHROrganization(string ParentUniqueID, string ParentID, List<HR_Organization> OrganizationList, DbEntities DB)
        {
            var organizationList = OrganizationList.Where(x => x.ParentID == ParentID).ToList();

            foreach (var organization in organizationList)
            {
                var o = DB.Organization.FirstOrDefault(x => x.ID == organization.ID);

                var uniqueID = Guid.NewGuid().ToString();

                if (o != null)
                {
                    uniqueID = o.UniqueID;
                    o.Description = organization.Name;
                }
                else
                {
                    DB.Organization.Add(new Organization()
                    {
                        UniqueID = uniqueID,
                        ID = organization.ID,
                        Description = organization.Name,
                        ParentUniqueID = ParentUniqueID
                    });
                }

                TransHROrganization(uniqueID, organization.ID, OrganizationList, DB);
            }
        }

        private void TransHRAccount(OracleConnection Conn)
        {
            try
            {
                using (OracleCommand cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CHIMEI.HR_ACCOUNT";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            adapter.Fill(dt);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                var accountList = dt.AsEnumerable().Select(x => new HR_Account
                                {
                                    ID = x["ID"].ToString(),
                                    Name = x["NAME"].ToString(),
                                    OrganizationID = x["ORGANIZATIONID"].ToString(),
                                    Email = x["EMAIL"].ToString()
                                }).ToList();

                                using (DbEntities db = new DbEntities())
                                {
                                    foreach (var account in accountList)
                                    {
                                        var a = db.User.FirstOrDefault(x => x.ID == account.ID);

                                        if (a != null)
                                        {
                                            a.Name = account.Name;
                                            a.Email = account.Email;
                                            a.LastModifyTime = DateTime.Now;
                                            a.OrganizationUniqueID = db.Organization.First(x => x.ID == account.OrganizationID).UniqueID;
                                        }
                                        else
                                        {
                                            db.User.Add(new User()
                                            {
                                                ID = account.ID,
                                                Name = account.Name,
                                                Password = account.ID,
                                                Email = account.Email,
                                                IsMobileUser = true,
                                                LastModifyTime = DateTime.Now,
                                                OrganizationUniqueID = db.Organization.First(x => x.ID == account.OrganizationID).UniqueID
                                            });

                                            db.UserAuthGroup.Add(new UserAuthGroup()
                                            {
                                                UserID = account.ID,
                                                AuthGroupID = "General"
                                            });
                                        }
                                    }

                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void TransOrganization(OracleConnection Conn)
        {
            try
            {
                using (OracleCommand cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CHIMEI.T_ORGANIZATION";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            adapter.Fill(dt);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                var organizationList = dt.AsEnumerable().Select(x => new T_Organization
                                {
                                    ID = x["UNITID"].ToString(),
                                    ParentID = x["PARENTUNITID"].ToString(),
                                    Name = x["UNITNAME"].ToString(),
                                    Type = x["UNITTYPE"].ToString()
                                }).ToList();

                                using (DbEntities db = new DbEntities())
                                {
                                    var ancestor = db.Organization.First(x => x.ParentUniqueID == "*");

                                    var divisionList = organizationList.Where(x => x.Type == "DIVISION").ToList();

                                    foreach (var division in divisionList)
                                    {
                                        var divisionUniqueID = Guid.NewGuid().ToString();

                                        var d = db.Organization.FirstOrDefault(x => x.ID == division.ID);

                                        if (d != null)
                                        {
                                            divisionUniqueID = d.UniqueID;
                                            d.Description = division.Name;
                                        }
                                        else
                                        {
                                            db.Organization.Add(new Organization()
                                            {
                                                UniqueID = divisionUniqueID,
                                                ID = division.ID,
                                                Description = division.Name,
                                                ParentUniqueID = ancestor.UniqueID
                                            });
                                        }

                                        var departmentList = organizationList.Where(x => x.Type == "DEPARTMENT" && x.ParentID == division.ID).ToList();

                                        foreach (var department in departmentList)
                                        {
                                            var departmentUniqueID = Guid.NewGuid().ToString();

                                            var p = db.Organization.FirstOrDefault(x => x.ID == department.ID);

                                            if (p != null)
                                            {
                                                departmentUniqueID = p.UniqueID;
                                                p.Description = department.Name;
                                            }
                                            else
                                            {
                                                db.Organization.Add(new Organization()
                                                {
                                                    UniqueID = departmentUniqueID,
                                                    ID = department.ID,
                                                    Description = department.Name,
                                                    ParentUniqueID = divisionUniqueID
                                                });
                                            }

                                            var sectionList = organizationList.Where(x => x.Type == "SECTION" && x.ParentID == department.ID).ToList();

                                            foreach (var section in sectionList)
                                            {
                                                var s = db.Organization.FirstOrDefault(x => x.ID == section.ID);

                                                if (s != null)
                                                {
                                                    s.Description = section.Name;
                                                }
                                                else
                                                {
                                                    db.Organization.Add(new Organization()
                                                    {
                                                        UniqueID = Guid.NewGuid().ToString(),
                                                        ID = section.ID,
                                                        Description = section.Name,
                                                        ParentUniqueID = departmentUniqueID
                                                    });
                                                }
                                            }
                                        }
                                    }

                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void TransEquipment(OracleConnection Conn)
        {
            try
            {
                var newEquipmentList = new List<Equipment>();

                using (OracleCommand cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CHIMEI.T_Equipment";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            adapter.Fill(dt);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                var equipmentList = dt.AsEnumerable().Select(x => new T_Equipment
                                {
                                    ID = x["ID"].ToString(),
                                    UnitID = x["UNITID"].ToString(),
                                    Name = x["NAME"].ToString(),
                                }).ToList();

                                using (DbEntities db = new DbEntities())
                                {
                                    using (EDbEntities edb = new EDbEntities())
                                    {
                                        var checkItemList = edb.CheckItem.Where(x => x.CheckType.StartsWith("馬達-")).OrderBy(x => x.ID).ToList();

                                        foreach (var equipment in equipmentList)
                                        {
                                            var organization = db.Organization.First(x => x.ID == equipment.UnitID);

                                            var query = edb.Equipment.FirstOrDefault(x => x.OrganizationUniqueID == organization.UniqueID && x.ID == equipment.ID);

                                            if (query == null)
                                            {
                                                var uniqueID = Guid.NewGuid().ToString();

                                                edb.ControlPoint.Add(new ControlPoint()
                                                {
                                                    UniqueID = uniqueID,
                                                    ID = equipment.ID,
                                                    Description = equipment.Name,
                                                    IsFeelItemDefaultNormal = false,
                                                    LastModifyTime = DateTime.Now,
                                                    OrganizationUniqueID = organization.UniqueID
                                                });

                                                var e = new Equipment()
                                                {
                                                    UniqueID = uniqueID,
                                                    ID = equipment.ID,
                                                    Name = equipment.Name,
                                                    IsFeelItemDefaultNormal = false,
                                                    OrganizationUniqueID = organization.UniqueID,
                                                    LastModifyTime = DateTime.Now
                                                };

                                                edb.Equipment.Add(e);
                                                newEquipmentList.Add(e);

                                                edb.EquipmentCheckItem.AddRange(checkItemList.Select(x => new EquipmentCheckItem
                                                {
                                                    EquipmentUniqueID = uniqueID,
                                                    CheckItemUniqueID = x.UniqueID,
                                                    AccumulationBase = null,
                                                    IsInherit = true,
                                                    LowerAlertLimit = null,
                                                    LowerLimit = null,
                                                    PartUniqueID = "*",
                                                    Remark = null,
                                                    Unit = null,
                                                    UpperAlertLimit = null,
                                                    UpperLimit = null,
                                                }).ToList());

                                                var route = edb.Route.FirstOrDefault(x => x.OrganizationUniqueID == organization.UniqueID && x.ID.EndsWith("-MOTOR"));

                                                var routeUniqueID = Guid.NewGuid().ToString();

                                                if (route != null)
                                                {
                                                    routeUniqueID = route.UniqueID;
                                                }
                                                else
                                                {
                                                    edb.Route.Add(new Route()
                                                    {
                                                        UniqueID = routeUniqueID,
                                                        ID = string.Format("{0}-MOTOR", organization.ID),
                                                        OrganizationUniqueID = organization.UniqueID,
                                                        Name = string.Format("{0}-馬達巡檢", organization.Description),
                                                        LastModifyTime = DateTime.Now
                                                    });

                                                    edb.RouteManager.Add(new RouteManager() {
                                                     RouteUniqueID = routeUniqueID,
                                                      UserID="10592"
                                                    });

                                                    edb.RouteManager.Add(new RouteManager() {
                                                        RouteUniqueID = routeUniqueID,
                                                        UserID = "A10008"
                                                    });
                                                }

                                                var controlPointQuery = edb.RouteControlPoint.Where(x => x.RouteUniqueID == routeUniqueID).OrderByDescending(x => x.Seq).ToList();

                                                var controlPointSeq = 1;

                                                if (controlPointQuery.Count > 0)
                                                {
                                                    controlPointSeq = controlPointQuery.First().Seq + 1;
                                                }

                                                edb.RouteControlPoint.Add(new RouteControlPoint()
                                                {
                                                    RouteUniqueID = routeUniqueID,
                                                    ControlPointUniqueID = uniqueID,
                                                    Seq = controlPointSeq
                                                });

                                                var equipmentQuery = edb.RouteEquipment.Where(x => x.RouteUniqueID == routeUniqueID).OrderByDescending(x => x.Seq).ToList();

                                                var equipmentSeq = 1;

                                                if (equipmentQuery.Count > 0)
                                                {
                                                    equipmentSeq = equipmentQuery.First().Seq + 1;
                                                }

                                                edb.RouteEquipment.Add(new RouteEquipment()
                                                {
                                                    RouteUniqueID = routeUniqueID,
                                                    ControlPointUniqueID = uniqueID,
                                                    EquipmentUniqueID = uniqueID,
                                                    PartUniqueID = "*",
                                                    Seq = equipmentSeq
                                                });

                                                int seq = 1;

                                                foreach (var checkItem in checkItemList)
                                                {
                                                    edb.RouteEquipmentCheckItem.Add(new RouteEquipmentCheckItem()
                                                    {
                                                        RouteUniqueID = routeUniqueID,
                                                        ControlPointUniqueID = uniqueID,
                                                        EquipmentUniqueID = uniqueID,
                                                        PartUniqueID = "*",
                                                        CheckItemUniqueID = checkItem.UniqueID,
                                                        Seq = seq
                                                    });

                                                    seq++;
                                                }

                                                edb.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (newEquipmentList != null && newEquipmentList.Count > 0)
                {
                    SendNewRFIDMail(newEquipmentList);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void TransDeletedEquipment(OracleConnection Conn)
        {
            try
            {
                var deletedEquipmentList = new List<Equipment>();

                using (OracleCommand cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CHIMEI.T_Equipment";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            adapter.Fill(dt);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                var aimsEquipmentList = dt.AsEnumerable().Select(x => new T_Equipment
                                {
                                    ID = x["ID"].ToString(),
                                    UnitID = x["UNITID"].ToString(),
                                    Name = x["NAME"].ToString(),
                                }).ToList();

                                using (DbEntities db = new DbEntities())
                                {
                                    using (EDbEntities edb = new EDbEntities())
                                    {
                                        var femEquipmentList = edb.Equipment.ToList();

                                        foreach (var equipment in femEquipmentList)
                                        {
                                            var organization = db.Organization.FirstOrDefault(x => x.UniqueID == equipment.OrganizationUniqueID);

                                            if (organization != null)
                                            {
                                                var query = aimsEquipmentList.FirstOrDefault(x => x.UnitID == organization.ID && x.ID == equipment.ID);

                                                if (query == null)
                                                {
                                                    deletedEquipmentList.Add(equipment);

                                                    edb.Equipment.RemoveRange(edb.Equipment.Where(x => x.UniqueID == equipment.UniqueID).ToList());
                                                    edb.EquipmentCheckItem.RemoveRange(edb.EquipmentCheckItem.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                                                    edb.ControlPoint.RemoveRange(edb.ControlPoint.Where(x => x.UniqueID == equipment.UniqueID).ToList());

                                                    edb.RouteControlPoint.RemoveRange(edb.RouteControlPoint.Where(x => x.ControlPointUniqueID == equipment.UniqueID).ToList());
                                                    edb.RouteEquipment.RemoveRange(edb.RouteEquipment.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());
                                                    edb.RouteEquipmentCheckItem.RemoveRange(edb.RouteEquipmentCheckItem.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                                                    edb.JobControlPoint.RemoveRange(edb.JobControlPoint.Where(x => x.ControlPointUniqueID == equipment.UniqueID).ToList());
                                                    edb.JobEquipment.RemoveRange(edb.JobEquipment.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());
                                                    edb.JobEquipmentCheckItem.RemoveRange(edb.JobEquipmentCheckItem.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                                                    edb.CHIMEI_JOB.RemoveRange(edb.CHIMEI_JOB.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());
                                                }
                                            }
                                        }

                                        edb.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

                if (deletedEquipmentList != null && deletedEquipmentList.Count > 0)
                {
                    SendDeletedRFIDMail(deletedEquipmentList);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void TransJob(OracleConnection Conn)
        {
            try
            {
                using (OracleCommand cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CHIMEI.T_Job";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            adapter.Fill(dt);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                var jobList = new List<Models.Job>();

                                using (EDbEntities db = new EDbEntities())
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        var equipmentID = dt.Rows[i]["EQUIPMENTID"].ToString();
                                        var date = dt.Rows[i]["MDATE"].ToString();
                                        var actID = dt.Rows[i]["ACTID"].ToString();
                                        var actDesc = dt.Rows[i]["ACT_DESC"].ToString();
                                        var actKey = dt.Rows[i]["ACT_KEY"].ToString();

                                        var tmp = actDesc.Substring(actDesc.IndexOf("-(") + 1);

                                        var cycle = tmp.Substring(1, 2);
                                        var content = tmp.Substring(4, tmp.IndexOf("-") - 4);
                                        var contents = content.Split('/');
                                        var motor = tmp.Substring(tmp.IndexOf("-") + 1, 2);

                                        if (motor != "A級" && motor != "B級" && motor != "C級" && motor != "D級")
                                        {
                                            Logger.Log(string.Format("Motor Error : {0}", motor));
                                        }
                                        else
                                        {
                                            var equipment = db.Equipment.FirstOrDefault(x => x.ID == equipmentID);

                                            if (equipment != null)
                                            {
                                                var query = db.RouteEquipment.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID);

                                                if (query != null)
                                                {
                                                    var job = jobList.FirstOrDefault(x => x.RouteUniqueID == query.RouteUniqueID && x.Date == date && x.Motor == motor && x.Cycle == cycle);

                                                    if (job == null)
                                                    {
                                                        var j = new Models.Job()
                                                        {
                                                            RouteUniqueID = query.RouteUniqueID,
                                                            Motor = motor,
                                                            Cycle = cycle,
                                                            Date = date,
                                                            Content = new List<string>()
                                                        };

                                                        j.EquipmentList.Add(new CHIMEI_Equipment()
                                                        {
                                                            UniqueID = equipment.UniqueID,
                                                            ACTID = actID,
                                                            ACTKEY = actKey,
                                                            ACTDESC = actDesc
                                                        });

                                                        foreach (var c in contents)
                                                        {
                                                            if (c.Contains("注"))
                                                            {
                                                                j.Content.Add("注油");
                                                            }
                                                            else if (c.Contains("振"))
                                                            {
                                                                j.Content.Add("振動");
                                                            }
                                                            else if (c.Contains("溫"))
                                                            {
                                                                j.Content.Add("溫度");
                                                            }
                                                            else if (c.Contains("異"))
                                                            {
                                                                j.Content.Add("異響");
                                                            }
                                                            else
                                                            {
                                                                Logger.Log(string.Format("Job Content Not Found : {0}", c));
                                                            }
                                                        }

                                                        jobList.Add(j);
                                                    }
                                                    else
                                                    {
                                                        job.EquipmentList.Add(new CHIMEI_Equipment
                                                        {
                                                            UniqueID = equipment.UniqueID,
                                                            ACTID = actID,
                                                            ACTKEY = actKey,
                                                            ACTDESC = actDesc
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.Log(string.Format("Route Equipment Not Found : {0}", equipmentID));
                                                }
                                            }
                                            else
                                            {
                                                Logger.Log(string.Format("Equipment Not Found : {0}", equipmentID));
                                            }
                                        }
                                    }

                                    foreach (var job in jobList)
                                    {
                                        var jobUniqueID = System.Guid.NewGuid().ToString();

                                        var description = string.Format("[{0}-{1}]{2}", job.Motor, job.Cycle, job.Date);

                                        var j = db.Job.FirstOrDefault(x => x.RouteUniqueID == job.RouteUniqueID && x.Description == description);

                                        var isCycleError = false;

                                        if (j != null)
                                        {
                                            jobUniqueID = j.UniqueID;
                                        }
                                        else
                                        {
                                            var endDate = DateTimeHelper.DateString2DateTime(job.Date);

                                            if (job.Cycle == "1M")
                                            {
                                                endDate = endDate.Value.AddMonths(1).AddDays(-1);
                                            }
                                            else if (job.Cycle == "3M")
                                            {
                                                endDate = endDate.Value.AddMonths(3).AddDays(-1);
                                            }
                                            else if (job.Cycle == "4M")
                                            {
                                                endDate = endDate.Value.AddMonths(4).AddDays(-1);
                                            }
                                            else
                                            {
                                                isCycleError = true;
                                                Logger.Log(string.Format("Cycle Error : {0}", job.Cycle));
                                            }

                                            if (!isCycleError)
                                            {
                                                db.Job.Add(new DbEntity.MSSQL.EquipmentMaintenance.Job()
                                                {
                                                    UniqueID = jobUniqueID,
                                                    RouteUniqueID = job.RouteUniqueID,
                                                    BeginDate = DateTimeHelper.DateString2DateTime(job.Date).Value,
                                                    EndDate = endDate,
                                                    //BeginTime = string.Empty,
                                                    CycleCount = int.Parse(job.Cycle.Substring(0, 1)),
                                                    CycleMode = "M",
                                                    //EndTime = string.Empty,
                                                    Description = description,
                                                    IsCheckBySeq = false,
                                                    IsNeedVerify = false,
                                                    IsShowPrevRecord = true,
                                                    LastModifyTime = DateTime.Now,
                                                    Remark = string.Empty,
                                                    TimeMode = 1
                                                });

                                                db.JobUser.Add(new JobUser()
                                                {
                                                    JobUniqueID = jobUniqueID,
                                                    UserID = "10592"
                                                });


                                                db.JobUser.Add(new JobUser()
                                                {
                                                    JobUniqueID = jobUniqueID,
                                                    UserID = "A10008"
                                                });
                                            }
                                        }

                                        if (!isCycleError)
                                        {
                                            foreach (var equipment in job.EquipmentList)
                                            {
                                                var q = db.JobControlPoint.FirstOrDefault(x => x.JobUniqueID == jobUniqueID && x.ControlPointUniqueID == equipment.UniqueID);

                                                if (q != null)
                                                {

                                                }
                                                else
                                                {
                                                    db.JobControlPoint.Add(new JobControlPoint()
                                                    {
                                                        JobUniqueID = jobUniqueID,
                                                        ControlPointUniqueID = equipment.UniqueID,
                                                        MinTimeSpan = null
                                                    });

                                                    db.JobEquipment.Add(new JobEquipment()
                                                    {
                                                        JobUniqueID = jobUniqueID,
                                                        ControlPointUniqueID = equipment.UniqueID,
                                                        EquipmentUniqueID = equipment.UniqueID,
                                                        PartUniqueID = "*"
                                                    });

                                                    db.CHIMEI_JOB.Add(new CHIMEI_JOB()
                                                    {
                                                        JobUniqueID = jobUniqueID,
                                                        ControlPointUniqueID = equipment.UniqueID,
                                                        EquipmentUniqueID = equipment.UniqueID,
                                                        PartUniqueID = "*",
                                                        ACT_ID = equipment.ACTID,
                                                        ACT_KEY = equipment.ACTKEY,
                                                        ACT_DESC = equipment.ACTDESC
                                                    });

                                                    var checkItemList = (from x in db.RouteEquipmentCheckItem
                                                                         join c in db.CheckItem
                                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                                         where x.RouteUniqueID == job.RouteUniqueID && x.ControlPointUniqueID == equipment.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                                                         select c).ToList();

                                                    foreach (var content in job.Content)
                                                    {
                                                        var c = checkItemList.Where(x => x.CheckType.Contains(content)).ToList();

                                                        db.JobEquipmentCheckItem.AddRange(c.Select(x => new JobEquipmentCheckItem
                                                        {
                                                            JobUniqueID = jobUniqueID,
                                                            ControlPointUniqueID = equipment.UniqueID,
                                                            EquipmentUniqueID = equipment.UniqueID,
                                                            PartUniqueID = "*",
                                                            CheckItemUniqueID = x.UniqueID
                                                        }).ToList());
                                                    }
                                                }
                                            }

                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                SendAbnormalMail(err);
            }
        }

        private void SendAbnormalMail(Error Error)
        { 
        
        }

        private void SendNewRFIDMail(List<Equipment> EquipmentList)
        {
            try
            {
                if (ToList.Count > 0)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var subject = string.Format("[設備新增通知]共{0}筆", EquipmentList.Count);

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<p>請至FEM設備保養管理系統建立設備RFID管制卡片TagID</p>");

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "組織"));
                        sb.Append(string.Format(th, "設備代號"));
                        sb.Append("</tr>");

                        foreach (var equipment in EquipmentList)
                        {
                            var organization = db.Organization.FirstOrDefault(x => x.UniqueID == equipment.OrganizationUniqueID);

                            sb.Append("<tr>");

                            sb.Append(string.Format(td, organization != null ? organization.Description : ""));
                            sb.Append(string.Format(td, equipment.ID));

                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");

                        MailHelper.SendMail(ToList, subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void SendDeletedRFIDMail(List<Equipment> EquipmentList)
        {
            try
            {
                if (ToList.Count > 0)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var subject = string.Format("[設備刪除通知]共{0}筆", EquipmentList.Count);

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<p>設備及工單資料已自動於FEM設備保養管理系統刪除</p>");
                        sb.Append("<p>請至現場移除設備RFID管制卡片</p>");

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "組織"));
                        sb.Append(string.Format(th, "設備代號"));
                        sb.Append("</tr>");

                        foreach (var equipment in EquipmentList)
                        {
                            var organization = db.Organization.FirstOrDefault(x => x.UniqueID == equipment.OrganizationUniqueID);

                            sb.Append("<tr>");

                            sb.Append(string.Format(td, organization != null ? organization.Description : ""));
                            sb.Append(string.Format(td, equipment.ID));

                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");

                        MailHelper.SendMail(ToList, subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        ~TransHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
