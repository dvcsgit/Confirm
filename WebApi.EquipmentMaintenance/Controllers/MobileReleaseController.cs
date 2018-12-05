using DataAccess.EquipmentMaintenance;
using Models.EquipmentMaintenance.MobileRelease;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.EquipmentMaintenance.Controllers
{
    public class MobileReleaseController : ApiController
    {
        public HttpResponseMessage Get(int ID)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var file = MobileReleaseDataAccessor.Get(ID);

                if (file != null)
                {
                    response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StreamContent(new FileStream(file.FullFileName, FileMode.Open));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = file.FileName
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