using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class AIMSJobModel
    {
        public string VHNO { get; set; }

        public string JobDecsription { get; set; }

        public string OrganizationDescription
        {
            get
            {
                if (JobList != null && JobList.Count > 0)
                {
                    var organizationList = JobList.Select(x=>x.OrganizationDescription).Distinct().OrderBy(x=>x).ToList();

                    var sb = new StringBuilder();

                    foreach (var organization in organizationList)
                    {
                        sb.Append(organization);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string MotorType
        {
            get
            {
                try
                {
                    var tmp = JobDecsription.Substring(JobDecsription.IndexOf("-(") + 1);

                    return tmp.Substring(tmp.IndexOf("-") + 1, 2);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string Cycle
        {
            get
            {
                try
                {
                    var tmp = JobDecsription.Substring(JobDecsription.IndexOf("-(") + 1);

                    return tmp.Substring(1, 2);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string Contents
        {
            get
            {
                try
                {
                    var tmp = JobDecsription.Substring(JobDecsription.IndexOf("-(") + 1);

                    var content = tmp.Substring(4, tmp.IndexOf("-") - 4);

                    var contents = content.Split('/');

                    if (contents.Count() > 0)
                    {
                        var sb = new StringBuilder();

                        foreach (var c in contents)
                        {
                            if (c.Contains("注"))
                            {
                                sb.Append("注油");
                                sb.Append("、");
                            }
                            else if (c.Contains("振"))
                            {
                                sb.Append("振動");
                                sb.Append("、");
                            }
                            else if (c.Contains("溫"))
                            {
                                sb.Append("溫度");
                                sb.Append("、");
                            }
                            else if (c.Contains("異"))
                            {
                                sb.Append("異響");
                                sb.Append("、");
                            }
                        }

                        if (sb.Length > 0)
                        {
                            sb.Remove(sb.Length - 1, 1);
                        }

                        return sb.ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public DateTime JobDate { get; set; }

        public string JobDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(JobDate);
            }
        }

        public DateTime JobEndDate { get; set; }

        public List<JobModel> JobList { get; set; }

        public List<EquipmentModel> EquipmentList
        {
            get
            {
                return JobList.SelectMany(x => x.EquipmentList).ToList();
            }
        }

        private List<UserModel> JobUserList
        {
            get
            {
                return JobList.SelectMany(x => x.JobUserList).Distinct().ToList();
            }
        }

        public string JobUsers
        {
            get
            {
                if (JobUserList != null && JobUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var user in JobUserList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool HaveAbnormal
        {
            get
            {
                return JobList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return JobList.Any(x => x.HaveAlert);
            }
        }

        public double CheckItemCount
        {
            get
            {
                return JobList.Sum(x => x.CheckItemCount);
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return JobList.Sum(x => x.CheckedItemCount);
            }
        }

        public bool IsCompleted
        {
            get
            {
                return CheckedItemCount >= CheckItemCount;
            }
        }

        private bool IsCycleEnd
        {
            get
            {
                if (DateTime.Compare(DateTime.Now, JobEndDate) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string CompleteRate
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return "-";
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        if (IsCycleEnd)
                        {
                            return Resources.Resource.UnPatrol;
                        }
                        else
                        {
                            return string.Format("{0}(0%)", Resources.Resource.Processing);
                        }
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Resources.Resource.Completed;
                        }
                        else
                        {
                            if (IsCycleEnd)
                            {
                                return string.Format("{0}({1})", Resources.Resource.Incomplete, (CheckedItemCount / CheckItemCount).ToString("#0.00%"));
                            }
                            else
                            {

                                return string.Format("{0}({1})", Resources.Resource.Processing, (CheckedItemCount / CheckItemCount).ToString("#0.00%"));
                            }
                        }
                    }
                }
            }
        }

        public string CompleteRateLabelClass
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return string.Empty;
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        if (IsCycleEnd)
                        {
                            return Define.Label_Color_Red_Class;
                        }
                        else
                        {
                            return Define.Label_Color_Blue_Class;
                        }
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Define.Label_Color_Green_Class;
                        }
                        else
                        {
                            if (IsCycleEnd)
                            {
                                return Define.Label_Color_Red_Class;
                            }
                            else
                            {

                                return Define.Label_Color_Blue_Class;
                            }
                        }
                    }
                }
            }
        }

        private List<UserModel> CheckUserList
        {
            get
            {
                return JobList.SelectMany(x => x.CheckUserList).Distinct().ToList();
            }
        }

        public string CheckUsers
        {
            get
            {
                if (CheckUserList != null && CheckUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var user in CheckUserList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public AIMSJobModel()
        {
            JobList = new List<JobModel>();
        }
    }
}
