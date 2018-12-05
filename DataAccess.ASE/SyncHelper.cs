using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.ASE.DataSync;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DataAccess.ASE
{
    public class JobQueryResult
    {
        public string UNIQUEID { get; set; }

        public string DESCRIPTION { get; set; }

        public string ID { get; set; }

        public string NAME { get; set; }

        public DateTime? BEGINDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        public int? CYCLECOUNT { get; set; }

        public string CYCLEMODE { get; set; }

        public string BEGINTIME { get; set; }

        public string ENDTIME { get; set; }
    }

    public class SyncHelper
    {
        private static string ConnString
        {
            get
            {
                OracleConnectionStringBuilder sb = new OracleConnectionStringBuilder();

#if QAS
                sb.DataSource = "(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.14.62.204)(PORT=1574)))(CONNECT_DATA=(SERVICE_NAME=eipcqas)))";
                sb.PersistSecurityInfo = true;
                sb.UserID = "EIPC_USER";
                sb.Password = "vmp6RU03";
#else
                sb.DataSource = "(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.14.62.224)(PORT=1573)))(CONNECT_DATA=(SERVICE_NAME=EIPC)))";
                sb.PersistSecurityInfo = true;
                sb.UserID = "EIPC_USER";
                sb.Password = "vmp6RU03";
#endif

                return sb.ToString();
            }
        }

        public static RequestResult GetJobList(string UserID, string CheckDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                Logger.Log(string.Format("SyncHelper.GetJobList(UserID={0}, CheckDate={1})", UserID, CheckDate));

                var itemList = new List<JobItem>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

                    #region Patrol
                    if (Config.Modules.Contains(Define.EnumSystemModule.EquipmentPatrol))
                    {
                        var jobList = new List<JobQueryResult>();

                        using (OracleConnection conn = new OracleConnection(ConnString))
                        {
                            conn.Open();

                            using (OracleCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT J.UNIQUEID, J.DESCRIPTION, R.ID, R.NAME, J.BEGINDATE, J.ENDDATE, J.CYCLECOUNT, J.CYCLEMODE, J.BEGINTIME, J.ENDTIME FROM EIPC.JOBUSER X, EIPC.JOB J, EIPC.ROUTE R WHERE X.JOBUNIQUEID = J.UNIQUEID AND J.ROUTEUNIQUEID = R.UNIQUEID AND X.USERID = '{0}'", UserID);

                                using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        jobList = dt.AsEnumerable().Select(x => new JobQueryResult
                                        {
                                            UNIQUEID = x["UNIQUEID"].ToString(),
                                            DESCRIPTION = x["DESCRIPTION"].ToString(),
                                            ID = x["ID"].ToString(),
                                            NAME = x["NAME"].ToString(),
                                            BEGINDATE = DateTime.Parse(x["BEGINDATE"].ToString()),
                                            ENDDATE = !string.IsNullOrEmpty(x["ENDDATE"].ToString()) ? DateTime.Parse(x["ENDDATE"].ToString()) : default(DateTime?),
                                            CYCLECOUNT = int.Parse(x["CYCLECOUNT"].ToString()),
                                            CYCLEMODE = x["CYCLEMODE"].ToString(),
                                            BEGINTIME = x["BEGINTIME"].ToString(),
                                            ENDTIME = x["ENDTIME"].ToString()
                                        }).ToList();
                                    }
                                }
                            }

                            conn.Close();
                        }

                        foreach (var job in jobList)
                        {
                            if (JobCycleHelper.IsInCycle(checkDate, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE))
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(checkDate, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out beginDate, out endDate);

                                var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                var jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);

                                itemList.Add(new JobItem()
                                {
                                    JobUniqueID = job.UNIQUEID,
                                    JobDescription = job.DESCRIPTION,
                                    RouteID = job.ID,
                                    RouteName = job.NAME,
                                    BeginDate = beginDate,
                                    BeginTime = job.BEGINTIME,
                                    EndDate = endDate,
                                    EndTime = job.ENDTIME,
                                    CheckItemCount = 0,
                                    CheckedItemCount = 0,
                                    CompleteRate = jobResult != null ? jobResult.COMPLETERATE : "-"
                                });
                            }
                        }
                    }
                    #endregion

                    #region Maintenance
                    if (Config.Modules.Contains(Define.EnumSystemModule.EquipmentMaintenance))
                    {
                        var mFormList = (from f in db.MFORM
                                         join x in db.MJOBUSER
                                         on f.MJOBUNIQUEID equals x.MJOBUNIQUEID
                                         where (f.STATUS == "0" || f.STATUS == "1" || f.STATUS == "4" || f.STATUS == "6") && x.USERID == UserID && DateTime.Compare(checkDate, f.ESTBEGINDATE) >= 0 && DateTime.Compare(checkDate, f.ESTBEGINDATE) <= 0
                                         select f.UNIQUEID).ToList();

                        foreach (var formUniqueID in mFormList)
                        {
                            var query = (from f in db.MFORM
                                         join j in db.MJOB
                                         on f.MJOBUNIQUEID equals j.UNIQUEID
                                         join e in db.EQUIPMENT
                                         on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                         join p in db.EQUIPMENTPART
                                         on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                         from p in tmpPart.DefaultIfEmpty()
                                         where f.UNIQUEID == formUniqueID
                                         select new
                                         {
                                             UniqueID = f.UNIQUEID,
                                             f.VHNO,
                                             f.ESTBEGINDATE,
                                             f.ESTENDDATE,
                                             EquipmentID = e.ID,
                                             EquipmentName = e.NAME,
                                             PartUniqueID = f.PARTUNIQUEID,
                                             PartDescription = p != null ? p.DESCRIPTION : "",
                                             Description = j.DESCRIPTION
                                         }).First();

                            itemList.Add(new JobItem()
                            {
                                MaintanenceFormUniqueID = query.UniqueID,
                                VHNO = query.VHNO,
                                FormType = Resources.Resource.MaintenanceForm,
                                Subject = query.Description,
                                BeginDate = query.ESTBEGINDATE,
                                EndDate = query.ESTENDDATE,
                                EquipmentID = query.EquipmentID,
                                EquipmentName = query.EquipmentName,
                                PartDescription = query.PartDescription
                            });
                        }

                        //Logger.Log("Get MForm Finished");

                        #region RepairForm
                        var repairFormList = (from f in db.RFORM
                                              join e in db.EQUIPMENT
                                              on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                              from e in tmpEquipment.DefaultIfEmpty()
                                              join p in db.EQUIPMENTPART
                                              on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                              from p in tmpPart.DefaultIfEmpty()
                                              join x in db.RFORMJOBUSER
                                              on f.UNIQUEID equals x.RFORMUNIQUEID
                                              join t in db.RFORMTYPE
                                              on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                                              where (f.STATUS == "2" || f.STATUS == "4" || f.STATUS == "7" || f.STATUS == "9") && x.USERID == UserID
                                              select new
                                              {
                                                  UniqueID = f.UNIQUEID,
                                                  RFormType = t.DESCRIPTION,
                                                  EstBeginDate = f.ESTBEGINDATE,
                                                  EstEndDate = f.ESTENDDATE,
                                                  VHNO = f.VHNO,
                                                  Subject = f.SUBJECT,
                                                  EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                                  EquipmentID = e != null ? e.ID : "",
                                                  EquipmentName = e != null ? e.NAME : "",
                                                  PartUniqueID = f.PARTUNIQUEID,
                                                  PartDescription = p != null ? p.DESCRIPTION : ""
                                              }).Distinct().ToList();

                        foreach (var repairForm in repairFormList)
                        {
                            var item = new JobItem()
                            {
                                RepairFormUniqueID = repairForm.UniqueID,
                                Subject = repairForm.Subject,
                                VHNO = repairForm.VHNO,
                                FormType = repairForm.RFormType,
                                BeginDate = repairForm.EstBeginDate,
                                EndDate = repairForm.EstEndDate,
                                EquipmentID = repairForm.EquipmentID,
                                EquipmentName = repairForm.EquipmentName,
                                PartDescription = repairForm.PartDescription
                            };
                        }

                        //Logger.Log("Get RForm Finished");
                        #endregion
                    }
                    #endregion
                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);

                System.Diagnostics.Process process = new System.Diagnostics.Process();

                process.StartInfo.FileName = @"D:\FEM\IISRecycle.lnk";

                process.Start();

                Logger.Log("Recycle IIS");
            }

            return result;
        }
    }
}
