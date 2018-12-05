using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TankPatrol.MobileRelease
{
    public class FileModel
    {
        public string ApkName { get; set; }

        public Define.EnumDevice Device { get; set; }

        public string Extension
        {
            get
            {
                if (Device == Define.EnumDevice.Android)
                {
                    return "apk";
                }
                else
                {
                    return "cab";
                }
            }
        }

        public string FileName
        {
            get
            {
                return ApkName + "." + Extension;
            }
        }

        public string FullFileName
        {
            get
            {
                return Path.Combine(Config.TankPatrolMobileReleaseFolderPath, FileName);
            }
        }

        public string ContentType
        {
            get
            {
                switch (Extension)
                {
                    case "apk":
                        return "application/vnd.android.package-archive";
                    case "cab":
                        return "application/vnd.ms-cab-compressed";
                    default:
                        return "application/octet-stream";
                }
            }
        }
    }
}
