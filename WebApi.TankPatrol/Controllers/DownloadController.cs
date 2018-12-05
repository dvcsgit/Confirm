using DataSync.TankPatrol;
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

namespace WebApi.TankPatrol.Controllers
{
    public class DownloadController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Sync(string UserID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                using (DownloadHelper helper = new DownloadHelper())
                {
                    RequestResult result = helper.Generate(UserID);

                    if (result.IsSuccess)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK);
                        response.Content = new StreamContent(new FileStream(result.Data.ToString(), FileMode.Open));
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = Define.SQLiteZip_TankPatrol
                        };
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
            }

            return response;
        }
    }
}