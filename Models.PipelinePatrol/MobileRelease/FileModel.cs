using System.IO;
using Utility;

namespace Models.PipelinePatrol.MobileRelease
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
                return Path.Combine(Config.PipelinePatrolMobileReleaseFolderPath, FileName);
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
