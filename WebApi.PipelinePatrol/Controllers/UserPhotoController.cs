using DataSync.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
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
    public class UserPhotoController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string UserID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                Logger.Log(UserID);

                RequestResult result = UserHelper.GetPhoto(UserID);

                if (result.IsSuccess)
                {
                    var photo = result.Data as PhotoModel;

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StreamContent(new FileStream(photo.FilePath, FileMode.Open));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = photo.FileName
                    };
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false });
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