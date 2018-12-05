using DataSync.PipelinePatrol;
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
using WebApi.PipelinePatrol.Utils;

namespace WebApi.PipelinePatrol.Controllers
{
    public class MessagePhotoController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetPhoto(string DialogUniqueID, int Seq)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(string.Format("DialogUniqueID:{0},Seq:{1}",DialogUniqueID,Seq));
                RequestResult result = ChatHelper.GetMessagePhoto(DialogUniqueID, Seq);
                

                if (result.IsSuccess)
                {
                    response = Request.CreateResponse(HttpStatusCode.OK);
                    string filePath = result.Data.ToString();
                    var fileInfo = new FileInfo(filePath);
                    Logger.Log(string.Format("filePath:{0}", filePath));    
                    response.Content = new StreamContent(new FileStream(filePath, FileMode.Open));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(AppUtils.GetMIMETypeString(fileInfo.Extension));
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileInfo.Name
                    };
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
                }
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