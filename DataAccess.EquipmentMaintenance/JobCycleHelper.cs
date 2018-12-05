using System;
using System.Reflection;
using Utility;

namespace DataAccess.EquipmentMaintenance
{
    public class JobCycleHelper
    {
        public static bool IsInCycle(DateTime BaseDate, DateTime BeginDate, DateTime? EndDate, int CycleCount, string CycleMode)
        {
            bool isInCycle = false;

            try
            {
                //還沒到派工開始日
                if (DateTime.Compare(BaseDate, BeginDate) < 0)
                {
                    isInCycle = false;
                }
                //已超過派工結束日
                else if (EndDate.HasValue && DateTime.Compare(BaseDate, EndDate.Value) > 0)
                {
                    isInCycle = false;
                }
                //是派工開始日
                else if (DateTime.Compare(BaseDate, BeginDate) == 0)
                {
                    isInCycle = true;
                }
                else
                {
                    DateTime beginDate, endDate;

                    //取得本次循環的起迄日
                    GetDateSpan(BaseDate, BeginDate, EndDate, CycleCount, CycleMode, out beginDate, out endDate);

                    //今天有落在起訖日內
                    if (BaseDate >= beginDate && BaseDate <= endDate)
                    {
                        isInCycle = true;
                    }
                    else
                    {
                        isInCycle = false;
                    }
                }
            }
            catch (Exception ex)
            {
                isInCycle = false;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return isInCycle;
        }

        public static void GetDateSpan(DateTime BaseDate, DateTime BeginDate,  DateTime? EndDate, int CycleCount, string CycleMode, out DateTime OutBeginDate, out DateTime OutEndDate)
        {
            if (BaseDate < BeginDate)
            {
                DateTime begin = BeginDate, end = DateTime.Today;

                switch (CycleMode)
                {
                    case "D":
                        end = begin.AddDays(CycleCount - 1);
                        break;
                    case "W":
                        end = begin.AddDays(CycleCount * 7 - 1);
                        break;
                    case "M":
                        end = begin.AddMonths(CycleCount).AddDays(-1);
                        break;
                    case "Y":
                        end = begin.AddYears(CycleCount).AddDays(-1);
                        break;
                }

                OutBeginDate = begin;
                OutEndDate = end;
            }
            else
            {
                DateTime begin = BeginDate, end = DateTime.Today;

                bool found = false;

                switch (CycleMode)
                {
                    case "D":
                        end = begin.AddDays(CycleCount - 1);

                        while (!found)
                        {
                            if (BaseDate >= begin && BaseDate <= end)
                            {
                                found = true;
                            }
                            else
                            {
                                begin = end.AddDays(1);
                                end = begin.AddDays(CycleCount - 1);
                            }
                        }
                        break;
                    case "W":
                        end = begin.AddDays(CycleCount * 7 - 1);

                        while (!found)
                        {
                            if (BaseDate >= begin && BaseDate <= end)
                            {
                                found = true;
                            }
                            else
                            {
                                begin = end.AddDays(1);
                                end = begin.AddDays(CycleCount * 7 - 1);
                            }
                        }
                        break;
                    case "M":
                        end = begin.AddMonths(CycleCount).AddDays(-1);

                        while (!found)
                        {
                            if (BaseDate >= begin && BaseDate <= end)
                            {
                                found = true;
                            }
                            else
                            {
                                begin = end.AddDays(1);
                                end = begin.AddMonths(CycleCount).AddDays(-1);
                            }
                        }
                        break;
                    case "Y":
                        end = begin.AddYears(CycleCount).AddDays(-1);

                        while (!found)
                        {
                            if (BaseDate >= begin && BaseDate <= end)
                            {
                                found = true;
                            }
                            else
                            {
                                begin = end.AddDays(1);
                                end = begin.AddYears(CycleCount).AddDays(-1);
                            }
                        }
                        break;
                }

                OutBeginDate = begin;
                OutEndDate = end;
            }
        }

        public static void GetDateTimeSpan(string CheckDate, string CheckTime, DateTime BeginDate, string BeginTime, DateTime? EndDate, string EndTime, int CycleCount, string CycleMode, out DateTime OutJobBeginTime, out string OutJobBeginDateString, out DateTime OutJobEndTime, out string OutJobEndDateString)
        {
            var checkTime = DateTimeHelper.DateTimeString2DateTime(CheckDate, CheckTime).Value;

            var jobBeginTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(BeginDate), BeginTime).Value;
            var jobEndTime = new DateTime();
            var tmp = new DateTime();

            if (checkTime < jobBeginTime)
            {
                switch (CycleMode)
                {
                    case "D":
                        tmp = jobBeginTime.AddDays(CycleCount - 1);
                        break;
                    case "W":
                        tmp = jobBeginTime.AddDays(CycleCount * 7 - 1);
                        break;
                    case "M":
                        tmp = jobBeginTime.AddMonths(CycleCount).AddDays(-1);
                        break;
                    case "Y":
                        tmp = jobBeginTime.AddYears(CycleCount).AddDays(-1);
                        break;
                }

                jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;

                //跨夜派工
                if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                {
                    jobEndTime = jobEndTime.AddDays(1);
                }
                //未指定派工時間
                else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                {
                    jobEndTime = jobEndTime.AddDays(1);
                }
            }
            else
            {
                bool found = false;

                switch (CycleMode)
                {
                    case "D":
                        tmp = jobBeginTime.AddDays(CycleCount - 1);

                        jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                        if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }
                        else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }

                        while (!found)
                        {
                            if (checkTime <= jobEndTime)
                            {
                                found = true;
                            }
                            else
                            {
                                jobBeginTime = tmp.AddDays(1);

                                tmp = jobBeginTime.AddDays(CycleCount - 1);
                                jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                                if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                                else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                            }
                        }
                        break;
                    case "W":
                        tmp = jobBeginTime.AddDays(CycleCount * 7 - 1);

                        jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                        if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }
                        else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }

                        while (!found)
                        {
                            if (checkTime <= jobEndTime)
                            {
                                found = true;
                            }
                            else
                            {
                                jobBeginTime = tmp.AddDays(1);

                                tmp = jobBeginTime.AddDays(CycleCount * 7 - 1);
                                jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                                if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                                else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                            }
                        }
                        break;
                    case "M":
                        tmp = jobBeginTime.AddMonths(CycleCount).AddDays(-1);

                        jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                        if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }
                        else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }

                        while (!found)
                        {
                            if (checkTime <= jobEndTime)
                            {
                                found = true;
                            }
                            else
                            {
                                jobBeginTime = tmp.AddDays(1);

                                tmp = jobBeginTime.AddMonths(CycleCount).AddDays(-1);
                                jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                                if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                                else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                            }
                        }
                        break;
                    case "Y":
                        tmp = jobBeginTime.AddYears(CycleCount).AddDays(-1);

                        jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                        if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }
                        else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                        {
                            jobEndTime = jobEndTime.AddDays(1);
                        }

                        while (!found)
                        {
                            if (checkTime <= jobEndTime)
                            {
                                found = true;
                            }
                            else
                            {
                                jobBeginTime = tmp.AddDays(1);
                                tmp = jobBeginTime.AddYears(CycleCount).AddDays(-1);
                                jobEndTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(tmp), EndTime).Value;
                                if (!string.IsNullOrEmpty(BeginTime) && !string.IsNullOrEmpty(EndTime) && string.Compare(BeginTime, EndTime) > 0)
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                                else if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
                                {
                                    jobEndTime = jobEndTime.AddDays(1);
                                }
                            }
                        }
                        break;
                }
            }

            OutJobBeginTime = jobBeginTime;
            OutJobEndTime = jobEndTime;

            OutJobBeginDateString = DateTimeHelper.DateTime2DateString(jobBeginTime);
            
            if (string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime))
            {
                OutJobEndDateString = DateTimeHelper.DateTime2DateString(jobEndTime.AddDays(-1));
            }
            else
            {
                OutJobEndDateString = DateTimeHelper.DateTime2DateString(jobEndTime);
            }
        }

        //public static void GetDateSpanByCheckTime(string CheckDate, string CheckTime, DateTime BeginDate, string BeginTime, DateTime? EndDate, string EndTime, int CycleCount, string CycleMode, out DateTime OutBeginDate, out DateTime OutEndDate)
        //{
        //    var checkTime = DateTimeHelper.DateTimeString2DateTime(CheckDate, CheckTime).Value;

        //    var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

        //    DateTime? prevEndTime = null;

        //    while (true)
        //    {
        //        DateTime begin, end;

        //        JobCycleHelper.GetDateSpan(checkDate, BeginDate, EndDate, CycleCount, CycleMode, out begin, out end);

        //        var beginTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(begin), BeginTime).Value;
        //        var endTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(end), EndTime).Value;

        //        if (string.Compare(BeginTime, EndTime) > 0)
        //        {
        //            endTime = endTime.AddDays(1);
        //        }

        //        if (checkTime >= beginTime)
        //            //if (checkTime >= beginTime && checkTime <= endTime)
        //        {
        //            OutBeginDate = beginTime;
        //            OutEndDate = endTime;

        //            break;
        //        }
        //        else
        //        {
        //            prevEndTime = endTime;
        //            checkDate = checkDate.AddDays(-1);
        //        }
        //    }
        //}
    }
}
