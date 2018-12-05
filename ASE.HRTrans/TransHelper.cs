using DataAccess.ASE;
using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Utility;

namespace ASE.HRTrans
{
    public class TransHelper : IDisposable
    {
         List<string> FACUserList = new List<string>();
         List<string> RMEHSUserList = new List<string>();

        public void Trans()
        {
            if (TransOrganization())
            {
                if (TransAccount())
                {
                    TransAbnormalNotifyGroup();

                    TransOrganizationManager();
                }
            }

            using (ASEDbEntities db = new ASEDbEntities())
            {
                db.Database.CommandTimeout = 600;

                var organizationList = db.ORGANIZATION.Select(x => x.UNIQUEID).ToList();

                var equipmentList = db.QA_EQUIPMENT.Where(x => !organizationList.Contains(x.ORGANIZATIONUNIQUEID)).ToList();

                foreach (var equipment in equipmentList)
                {
                    if (!string.IsNullOrEmpty(equipment.OWNERID))
                    {
                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == equipment.OWNERID);

                        if (owner != null)
                        {
                            equipment.ORGANIZATIONUNIQUEID = owner.ORGANIZATIONUNIQUEID;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(equipment.PEID))
                        {
                            var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == equipment.PEID);

                            if (pe != null)
                            {
                                equipment.ORGANIZATIONUNIQUEID = pe.ORGANIZATIONUNIQUEID;
                            }
                        }
                    }
                }

                var calibrationApplyList = db.QA_CALIBRATIONAPPLY.Where(x => !organizationList.Contains(x.ORGANIZATIONUNIQUEID)).ToList();

                foreach (var calibrationApply in calibrationApplyList)
                {
                    if (!string.IsNullOrEmpty(calibrationApply.OWNERID))
                    {
                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == calibrationApply.OWNERID);

                        if (owner != null)
                        {
                            calibrationApply.ORGANIZATIONUNIQUEID = owner.ORGANIZATIONUNIQUEID;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(calibrationApply.PEID))
                        {
                            var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == calibrationApply.PEID);

                            if (pe != null)
                            {
                                calibrationApply.ORGANIZATIONUNIQUEID = pe.ORGANIZATIONUNIQUEID;
                            }
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private bool TransOrganization()
        {
            bool result = false;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.Database.CommandTimeout = 600;
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        var departmentList = db.HR_DEPARTMENT.ToList();

                        Logger.Log(string.Format("HR Department Count : {0}", departmentList.Count));

                        var ancestors = departmentList.Where(x => x.PARENTID == "日月光集團(實體組織)").ToList();

                        foreach (var ancestor in ancestors)
                        {
                            TransOrganization(db, ancestor, departmentList);
                        }

                        var query1 = (from o in db.ORGANIZATION
                                      join p in db.ORGANIZATION
                                      on o.PARENTUNIQUEID equals p.UNIQUEID
                                      where o.HR == "Y"
                                      select new
                                      {
                                          UniqueID = o.UNIQUEID,
                                          ParentID = p.ID,
                                          ID = o.ID
                                      }).ToList();

                        foreach (var q in query1)
                        {
                            if (!departmentList.Any(x => x.PARENTID == q.ParentID && x.ID == q.ID))
                            {
                                var organization = db.ORGANIZATION.First(x => x.UNIQUEID == q.UniqueID);

                                db.ORGANIZATION.Remove(organization);

                                db.SaveChanges();

                                Logger.Log(string.Format("Remove Organization {0}/{1}/{2}", organization.UNIQUEID, organization.ID, organization.DESCRIPTION));
                            }
                        }

                        var organizationUniqueIDList = db.ORGANIZATION.Where(x => x.HR == "Y").Select(x => x.UNIQUEID).ToList();

                        var deleteOrganizationList = db.ORGANIZATION.Where(x => x.PARENTUNIQUEID != "*" && x.HR == "Y" && !organizationUniqueIDList.Contains(x.PARENTUNIQUEID)).ToList();

                        while (deleteOrganizationList.Count > 0)
                        {
                            foreach (var deleteOrganization in deleteOrganizationList)
                            {
                                var query = OrganizationDataAccessor.GetDownStreamOrganizationList(deleteOrganization.UNIQUEID, true);

                                var downStreamOrganizationList = db.ORGANIZATION.Where(x => query.Contains(x.UNIQUEID)).ToList();

                                foreach (var downStreamOrganization in downStreamOrganizationList)
                                {
                                    db.ORGANIZATION.Remove(downStreamOrganization);

                                    db.SaveChanges();

                                    Logger.Log(string.Format("Remove Organization {0}/{1}/{2}", downStreamOrganization.UNIQUEID, downStreamOrganization.ID, downStreamOrganization.DESCRIPTION));
                                }
                            }

                            organizationUniqueIDList = db.ORGANIZATION.Where(x => x.HR == "Y").Select(x => x.UNIQUEID).ToList();

                            deleteOrganizationList = db.ORGANIZATION.Where(x => x.PARENTUNIQUEID != "*" && x.HR == "Y" && !organizationUniqueIDList.Contains(x.PARENTUNIQUEID)).ToList();
                        }
#if !DEBUG
                        trans.Complete();
                    }
#endif
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }

        private void TransOrganization(ASEDbEntities DB, HR_DEPARTMENT Ancestor, List<HR_DEPARTMENT> DepartmentList)
        {
            var uniqueID = Guid.NewGuid().ToString();

            var organization = DB.ORGANIZATION.FirstOrDefault(x => x.HR == "Y" && x.PARENTUNIQUEID == "*" && x.ID == Ancestor.ID);

            if (organization != null)
            {
                uniqueID = organization.UNIQUEID;
                organization.DESCRIPTION = Ancestor.NAME;
            }
            else
            {
                DB.ORGANIZATION.Add(new ORGANIZATION()
                {
                    UNIQUEID = uniqueID,
                    ID = Ancestor.ID,
                    PARENTUNIQUEID = "*",
                    DESCRIPTION = Ancestor.NAME,
                    HR = "Y"
                });

                Logger.Log(string.Format("Add Organization {0}/{1}/{2}", uniqueID, Ancestor.ID, Ancestor.NAME));
            }

            DB.SaveChanges();

            TransOrganization(DB, uniqueID, Ancestor, DepartmentList);
        }

        private void TransOrganization(ASEDbEntities DB, string ParentUniqueID, HR_DEPARTMENT Department, List<HR_DEPARTMENT> DepartmentList)
        {
            var departmentList = DepartmentList.Where(x => x.PARENTID == Department.ID).ToList();

            foreach (var department in departmentList)
            {
                var uniqueID = Guid.NewGuid().ToString();

                var organization = DB.ORGANIZATION.FirstOrDefault(x => x.HR == "Y" && x.PARENTUNIQUEID == ParentUniqueID && x.ID == department.ID);

                if (organization != null)
                {
                    uniqueID = organization.UNIQUEID;

                    organization.DESCRIPTION = department.NAME;
                }
                else
                {
                    DB.ORGANIZATION.Add(new ORGANIZATION()
                    {
                        UNIQUEID = uniqueID,
                        ID = department.ID,
                        PARENTUNIQUEID = ParentUniqueID,
                        DESCRIPTION = department.NAME,
                        HR = "Y"
                    });

                    Logger.Log(string.Format("Add Organization {0}/{1}/{2}", uniqueID, department.ID, department.NAME));
                }

                DB.SaveChanges();

                TransOrganization(DB, uniqueID, department, DepartmentList);
            }
        }

        private bool TransAccount()
        {
            bool result = false;

            try
            {
                var facOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList("79bddfd6-8a1b-4742-9f3e-5e77931899c7", true);
                
                var facUserOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList("79bddfd6-8a1b-4742-9f3e-5e77931899c7", false);
                
                var rmehsUserOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList("14ee805b-bc85-4b90-9b1b-e29ea5ba970c", false);
                

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.Database.CommandTimeout = 600;
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        var employeeList = db.HR_EMPLOYEE.ToList();

                        Logger.Log(string.Format("HR Employee Count : {0}", employeeList.Count));

                        foreach (var employee in employeeList)
                        {
                            var user = db.ACCOUNT.FirstOrDefault(x => x.ID == employee.ID);

                            if (user == null)
                            {
                                var organization = db.ORGANIZATION.FirstOrDefault(x => x.ID == employee.DEPARTMENTID);

                                if (organization != null)
                                {
                                    db.ACCOUNT.Add(new ACCOUNT()
                                    {
                                        ID = employee.ID,
                                        ORGANIZATIONUNIQUEID = organization.UNIQUEID,
                                        EMAIL = !string.IsNullOrEmpty(employee.EMAIL) ? employee.EMAIL.Replace(' ', '_') : employee.EMAIL,
                                        ISMOBILEUSER = "N",
                                        LASTMODIFYTIME = DateTime.Now,
                                        NAME = employee.NAME,
                                        PASSWORD = employee.ID,
                                        TITLE = employee.TITLE,
                                        LOGINID = null,
                                        TAGID = null,
                                        MANAGERID = employee.MANAGERID
                                    });

                                    if (!db.USERAUTHGROUP.Any(x => x.USERID == employee.ID && x.AUTHGROUPID == "General"))
                                    {
                                        db.USERAUTHGROUP.Add(new USERAUTHGROUP()
                                        {
                                            USERID = employee.ID,
                                            AUTHGROUPID = "General"
                                        });
                                    }

                                    if (facOrganizationList.Contains(organization.UNIQUEID))
                                    {
                                        if (!db.USERAUTHGROUP.Any(x => x.USERID == employee.ID && x.AUTHGROUPID == "FAC_General"))
                                        {
                                            db.USERAUTHGROUP.Add(new USERAUTHGROUP()
                                            {
                                                USERID = employee.ID,
                                                AUTHGROUPID = "FAC_General"
                                            });
                                        }
                                    }

                                    db.SaveChanges();

                                    if (facUserOrganizationList.Contains(organization.UNIQUEID))
                                    {
                                        FACUserList.Add(employee.ID);
                                    }

                                    if (rmehsUserOrganizationList.Contains(organization.UNIQUEID))
                                    {
                                        RMEHSUserList.Add(employee.ID);
                                    }

                                    Logger.Log(string.Format("Add Account {0}/{1}", employee.ID, employee.NAME));
                                }
                                else
                                {
                                    Logger.Log(string.Format("New Employee But Organization Not Found {0}/{1} {2}", employee.ID, employee.NAME, employee.DEPARTMENTID));
                                }
                            }
                            else
                            {
                                var organization = db.ORGANIZATION.FirstOrDefault(x => x.ID == employee.DEPARTMENTID);

                                if (organization != null)
                                {
                                    user.ORGANIZATIONUNIQUEID = organization.UNIQUEID;
                                    user.EMAIL = !string.IsNullOrEmpty(employee.EMAIL) ? employee.EMAIL.Replace(' ', '_') : employee.EMAIL;
                                    user.LASTMODIFYTIME = DateTime.Now;
                                    user.NAME = employee.NAME;
                                    user.TITLE = employee.TITLE;
                                    user.MANAGERID = employee.MANAGERID;

                                    db.SaveChanges();

                                    if (facUserOrganizationList.Contains(organization.UNIQUEID))
                                    {
                                        FACUserList.Add(employee.ID);
                                    }

                                    if (rmehsUserOrganizationList.Contains(organization.UNIQUEID))
                                    {
                                        RMEHSUserList.Add(employee.ID);
                                    }
                                }
                                else
                                {
                                    Logger.Log(string.Format("Old Employee But Organization Not Found {0}/{1} {2}", employee.ID, employee.NAME, employee.DEPARTMENTID));
                                }
                            }
                        }

                        

                        var employeeIDList = employeeList.Select(e => e.ID).ToList();

                        var deleteAccountList = db.ACCOUNT.Where(x => x.ID != "admin" && !employeeIDList.Contains(x.ID)).ToList();

                        foreach (var deleteAccount in deleteAccountList)
                        {
                            Logger.Log(string.Format("Delete Account {0}/{1}", deleteAccount.ID, deleteAccount.NAME));

                            db.ACCOUNT.Remove(deleteAccount);

                            db.SaveChanges();
                        }

                        db.SaveChanges();

                    #if !DEBUG
                        trans.Complete();
                    }
#endif
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }

        private void TransAbnormalNotifyGroup()
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.Database.CommandTimeout = 600;
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    db.FAC_ABNORMALNOTIFYGROUPUSER.RemoveRange(db.FAC_ABNORMALNOTIFYGROUPUSER.Where(x => x.GROUPUNIQUEID == "fbbee973-e37d-42e5-834f-989395690cbf").ToList());
                    db.FAC_ABNORMALNOTIFYGROUPUSER.RemoveRange(db.FAC_ABNORMALNOTIFYGROUPUSER.Where(x => x.GROUPUNIQUEID == "d0765024-eb9c-47d6-a349-8c566d0ce238").ToList());

                    db.SaveChanges();

                    db.FAC_ABNORMALNOTIFYGROUPUSER.AddRange(FACUserList.Select(x => new FAC_ABNORMALNOTIFYGROUPUSER
                    {
                        GROUPUNIQUEID = "fbbee973-e37d-42e5-834f-989395690cbf",
                        USERID = x
                    }).ToList());

                    db.FAC_ABNORMALNOTIFYGROUPUSER.AddRange(RMEHSUserList.Select(x => new FAC_ABNORMALNOTIFYGROUPUSER
                    {
                        GROUPUNIQUEID = "d0765024-eb9c-47d6-a349-8c566d0ce238",
                        USERID = x
                    }).ToList());

                    db.SaveChanges();
#if !DEBUG
                        trans.Complete();
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        static void TransOrganizationManager()
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.Database.CommandTimeout = 600;
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        var organizationList = db.ORGANIZATION.Where(x => x.HR == "Y").ToList();

                        var accountList = db.ACCOUNT.ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStream = organizationList.Where(x => x.PARENTUNIQUEID == organization.UNIQUEID).Select(x => x.UNIQUEID).ToList();

                            if (downStream.Count > 0)
                            {
                                var sb = new StringBuilder();

                                foreach (var d in downStream)
                                {
                                    sb.Append("'");
                                    sb.Append(d);
                                    sb.Append("',");
                                }

                                sb.Remove(sb.Length - 1, 1);

                                var o = sb.ToString();

                                string sql = string.Format("SELECT MANAGERID, COUNT(*) CNT FROM EIPC.ACCOUNT WHERE ORGANIZATIONUNIQUEID in ({0}) GROUP BY MANAGERID", o);

                                var temp = db.Database.SqlQuery<ManagerCount>(sql).OrderByDescending(x => x.CNT).FirstOrDefault();

                                if (temp != null)
                                {
                                    organization.MANAGERUSERID = temp.ManagerID;

                                    //if (!db.ORGANIZATIONMANAGER.Any(x => x.ORGANIZATIONUNIQUEID == organization.UNIQUEID && x.USERID == temp.ManagerID))
                                    //{
                                    //    db.ORGANIZATIONMANAGER.Add(new ORGANIZATIONMANAGER()
                                    //    {
                                    //        ORGANIZATIONUNIQUEID = organization.UNIQUEID,
                                    //        USERID = temp.ManagerID
                                    //    });
                                    //}

                                    db.SaveChanges();
                                }
                                else
                                {
                                    temp = db.Database.SqlQuery<ManagerCount>(string.Format("SELECT MANAGERID, COUNT(*) CNT FROM EIPC.ACCOUNT WHERE ORGANIZATIONUNIQUEID = '{0}' GROUP BY MANAGERID", organization.UNIQUEID)).OrderByDescending(x => x.CNT).FirstOrDefault();

                                    if (temp != null)
                                    {
                                        organization.MANAGERUSERID = temp.ManagerID;

                                        //if (!db.ORGANIZATIONMANAGER.Any(x => x.ORGANIZATIONUNIQUEID == organization.UNIQUEID && x.USERID == temp.ManagerID))
                                        //{
                                        //    db.ORGANIZATIONMANAGER.Add(new ORGANIZATIONMANAGER()
                                        //    {
                                        //        ORGANIZATIONUNIQUEID = organization.UNIQUEID,
                                        //        USERID = temp.ManagerID
                                        //    });
                                        //}

                                        db.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                var temp = db.Database.SqlQuery<ManagerCount>(string.Format("SELECT MANAGERID, COUNT(*) CNT FROM EIPC.ACCOUNT WHERE ORGANIZATIONUNIQUEID = '{0}' GROUP BY MANAGERID", organization.UNIQUEID)).OrderByDescending(x => x.CNT).FirstOrDefault();

                                if (temp != null)
                                {
                                    organization.MANAGERUSERID = temp.ManagerID;

                                    //if (!db.ORGANIZATIONMANAGER.Any(x => x.ORGANIZATIONUNIQUEID == organization.UNIQUEID && x.USERID == temp.ManagerID))
                                    //{
                                    //    db.ORGANIZATIONMANAGER.Add(new ORGANIZATIONMANAGER()
                                    //    {
                                    //        ORGANIZATIONUNIQUEID = organization.UNIQUEID,
                                    //        USERID = temp.ManagerID
                                    //    });
                                    //}

                                    db.SaveChanges();
                                }
                            }
                        }
#if !DEBUG
                        trans.Complete();
                    }
#endif
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
