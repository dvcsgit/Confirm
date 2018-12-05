#if ASE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using WebSite.Areas.Customized_ASE_QA.Views.Utils;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class UtilsController : Controller
    {
        /// <summary>
        /// 檔案下載
        /// </summary>
        /// <param name="guidFileName">在temp資料夾產生的唯一識別</param>
        /// <param name="desFileName">給前端下載的檔名</param>
        /// <returns></returns>
        public ActionResult DownloadFile(string guidFileName, string desFileName)
        {
            try
            {
                var tempFilePath = Path.Combine(Config.TempFolder, guidFileName);
                FileStream fileStream = new FileStream(tempFilePath, FileMode.Open);
                

                var tempFileName = Path.GetFileName(tempFilePath);
                string fileDownloadName = desFileName;
                if (Tools.IsBrowserIE())
                {
                    fileDownloadName = Server.UrlPathEncode(desFileName);
                }
                return File(fileStream, Tools.GetMIMETypeString(guidFileName), fileDownloadName);
            }
            catch (Exception ex)
            {
                //_logger.Error("DownloadFile 發生錯誤:{0}", e.Message);
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);
                Logger.Log(err);
            }

            return Content("");
        }


        public ActionResult Download(string FullFileName)
        {
            try
            {
                FileStream fileStream = new FileStream(FullFileName, FileMode.Open);

                var fileInfo = new FileInfo(FullFileName);

                string fileDownloadName = fileInfo.Name;

                if (Tools.IsBrowserIE())
                {
                    fileDownloadName = Server.UrlPathEncode(fileDownloadName);
                }

                return File(fileStream, Tools.GetMIMETypeString(FullFileName), fileDownloadName);
            }
            catch (Exception ex)
            {
                //_logger.Error("DownloadFile 發生錯誤:{0}", e.Message);
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);
                Logger.Log(err);
            }

            return Content("");
        }
    }
}
#endif
