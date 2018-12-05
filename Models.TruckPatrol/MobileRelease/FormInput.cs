using System;
using System.Web;
using Utility;

namespace Models.TruckPatrol.MobileRelease
{
    public class FormInput
    {
        public string ApkName { get; set; }

        public string AppName
        {
            get
            {
                //tw.com.efpg.guard_v1.0.6.1_8_fpg_release
                if (!string.IsNullOrEmpty(ApkName) && ApkName.Contains("_"))
                {
                    //tw.com.efpg.guard
                    return ApkName.Substring(0, ApkName.IndexOf("_"));
                }
                else
                {
                    return ApkName;
                }
            }
        }

        public string VerName
        {
            get
            {
                //tw.com.efpg.guard_v1.0.6.1_8_fpg_release
                if (!string.IsNullOrEmpty(ApkName) && ApkName.Contains("_"))
                {
                    //1.0.6.1_8_fpg_release
                    string temp = ApkName.Substring(ApkName.IndexOf("_v") + 2);

                    //1.0.6.1
                    return temp.Substring(0, temp.IndexOf("_"));
                }
                else
                {
                    return ApkName;
                }
            }
        }

        public int VerCode
        {
            get
            {
                if (!string.IsNullOrEmpty(ApkName) && ApkName.Contains("_"))
                {
                    //1.0.6.1_8_fpg_release
                    string temp = ApkName.Substring(ApkName.IndexOf("_v") + 2);

                    //8_fpg_release
                    string temp2 = temp.Substring(temp.IndexOf("_") + 1);

                    //8
                    return int.Parse(temp2.Substring(0, temp2.IndexOf("_")));
                }
                else
                {
                    return -1;
                }
            }
        }

        public string ReleaseNote { get; set; }

        public bool IsForceUpdate { get; set; }

        public string ReleaseDateString { get; set; }

        public DateTime ReleaseDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(ReleaseDateString).Value;
            }
        }

        public Define.EnumDevice Device { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}
