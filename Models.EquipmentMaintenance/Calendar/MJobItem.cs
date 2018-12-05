using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.Calendar
{
    public class MJobItem
    {
        public string UniqueID { get; set; }

        public string Display
        {
            get
            {
                return string.Format("【預防保養作業】[{0}]{1}({2})", VHNO, Subject, StatusDescription);
            }
        }

        public string VHNO
        {
            get
            {
                if (FormList != null && FormList.Count > 0)
                {
                    return FormList.First().VHNO.Substring(0, 8);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Subject { get; set; }

        public List<string> StatusCodeList
        {
            get
            {
                return FormList.Select(x => x.StatusCode).Distinct().OrderBy(x => x).ToList();
            }
        }

        public string StatusDescription
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var statusCode in StatusCodeList)
                {
                    var statusDescription = string.Empty;

                    switch (statusCode)
                    {
                        case "0":
                            if (DateTime.Compare(DateTime.Today, EstBeginDate) > 0)
                            {
                                statusDescription = string.Format("{0}({1})", Resources.Resource.MFormStatus_0, "逾期");
                            }
                            else
                            {
                                statusDescription = Resources.Resource.MFormStatus_0;
                            }
                            break;
                        case "1":
                            statusDescription = Resources.Resource.MFormStatus_1;
                            break;
                        case "2":
                            statusDescription = Resources.Resource.MFormStatus_2;
                            break;
                        case "3":
                            statusDescription = Resources.Resource.MFormStatus_3;
                            break;
                        case "4":
                            statusDescription = Resources.Resource.MFormStatus_4;
                            break;
                        case "5":
                            statusDescription = Resources.Resource.MFormStatus_5;
                            break;
                        case "6":
                            statusDescription = Resources.Resource.MFormStatus_6;
                            break;
                        default:
                            statusDescription = string.Empty;
                            break;
                    }

                    if (!string.IsNullOrEmpty(statusDescription))
                    {
                        sb.Append(string.Format("{0}({1})", statusDescription, FormList.Count(x => x.StatusCode == statusCode)));
                        sb.Append("/");
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public DateTime CycleBeginDate { get; set; }

        public DateTime CycleEndDate { get; set; }

        public DateTime EstBeginDate
        {
            get
            {
                return FormList.Min(x => x.EstBeginDate);
            }
        }

        public string Begin
        {
            get
            {
                return EstBeginDate.ToString("s");
            }
        }

        public DateTime EstEndDate
        {
            get
            {
                return FormList.Max(x => x.EstEndDate);
            }
        }

        public string End
        {
            get
            {
                return EstEndDate.ToString("s");
            }
        }

        public string Color
        {
            get
            {
                if (StatusCodeList.Any(x => x == "2" || x == "4"))
                {
                    return Define.Color_Red;
                }
                else if (StatusCodeList.Any(x => x == "0"))
                {
                    if (DateTime.Compare(DateTime.Today, EstBeginDate) > 0)
                    {
                        return Define.Color_Red;
                    }
                    else
                    {
                        return Define.Color_Orange;
                    }
                }
                else if (StatusCodeList.Any(x => x == "1"))
                {
                    return Define.Color_Blue;
                }
                else if (StatusCodeList.Any(x => x == "3" || x == "6"))
                {
                    return Define.Color_Purple;
                }
                else if (StatusCodeList.Any(x => x == "5"))
                {
                    return Define.Color_Green;
                }
                else
                {
                    return Define.Color_Gray;
                }
            }
        }

        public List<MFormItem> FormList { get; set; }

        public MJobItem()
        {
            FormList = new List<MFormItem>();
        }
    }
}
