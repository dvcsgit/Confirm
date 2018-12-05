using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebSite.Areas.Customized_ASE_QA.Views.Utils
{
    public class Tools
    {
        /// <summary>
        /// 檢查是否為IE瀏覽器 IE11 也可以偵測到
        /// </summary>
        /// <returns>True:IE False:Chrome or FF</returns>
        public static bool IsBrowserIE()
        {
            var result = false;
            var request = HttpContext.Current.Request;
            if (request.Browser.Type.Contains("IE") || request.Browser.Type == "InternetExplorer11")
            {
                result = true;
            }
            else
            {
                //額外再做一個檢查
                var flag = Regex.IsMatch(request.UserAgent, @"(?:\b(MS)?IE\s+|\bTrident/7.0;.*\s+rv:)(\d+)");
                if (flag)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 取得指定附檔名的 MIME type string
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static string GetMIMETypeString(string fileExtension)
        {
            // 附檔名要是如 .bmp 格式
            fileExtension = fileExtension.Trim();
            if (fileExtension.StartsWith("."))
                fileExtension = "." + fileExtension;

            // 取得 MIME type string
            try
            {
                String mime = "application/octetstream";
                Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("." + fileExtension);
                if (rk != null && rk.GetValue("Content Type") != null)
                    mime = rk.GetValue("Content Type").ToString();
                return mime;
            }
            catch
            {
                return "application/octetstream";
            }
        }
    }
}