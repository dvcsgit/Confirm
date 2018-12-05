using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.TruckPatrol.Controllers
{
    public class RFIDDownloadController : ApiController
    {
        //[HttpGet]
        //public HttpResponseMessage Sync(string OrganizationUniqueID)
        //{
        //    HttpResponseMessage response = new HttpResponseMessage();

        //    try
        //    {
        //        using (RFIDDownloadHelper helper = new RFIDDownloadHelper())
        //        {
        //            RequestResult result = helper.Generate(OrganizationUniqueID);

        //            if (result.IsSuccess)
        //            {
        //                response = Request.CreateResponse(HttpStatusCode.OK);
        //                response.Content = new StreamContent(new FileStream(result.Data.ToString(), FileMode.Open));
        //                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        //                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        //                {
        //                    FileName = "TruckCheck.RFID.zip"
        //                };
        //            }
        //            else
        //            {
        //                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
        //    }

        //    return response;
        //}
    }
}