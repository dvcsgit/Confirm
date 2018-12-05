using System;
using System.Reflection;
using Utility;

namespace DataAccess.GuardPatrol
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

        public static void GetDateSpan(DateTime BaseDate, DateTime BeginDate, DateTime? EndDate, int CycleCount, string CycleMode, out DateTime OutBeginDate, out DateTime OutEndDate)
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
    }
}
