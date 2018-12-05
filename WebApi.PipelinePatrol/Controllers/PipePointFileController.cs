using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.PipelinePatrol.Controllers
{
    /// <summary>
    /// 取得 管缐檔案 附件
    /// </summary>
    public class PipePointFileController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string UniqueID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(UniqueID);

                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(new FileStream(Path.Combine(Config.PipelinePatrolFileFolderPath, string.Format("{0}.zip", UniqueID)), FileMode.Open));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Define.FileZip
                };
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
            }

            return response;
        }
    }
}