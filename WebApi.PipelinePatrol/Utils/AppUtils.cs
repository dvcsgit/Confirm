using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WebApi.PipelinePatrol.Utils
{
    public class AppUtils
    {
        /// <summary>
        /// 取得指定附檔名的 MIME type string
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static string GetMIMETypeString(string fileName)
        {

            // 取得 MIME type string
            try
            {
                string mimeType = MimeMapping.GetMimeMapping(fileName);
                return mimeType;
            }
            catch
            {
                return "application/octetstream";
            }
        }

        /// <summary>
        /// 取得Web.config中的AppSetting參數
        /// Usage: SiteLibrary.AppSettings("key")
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static String AppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}