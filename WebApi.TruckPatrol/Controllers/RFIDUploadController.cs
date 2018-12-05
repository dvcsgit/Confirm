using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Utility;
using Utility.Models;
namespace WebApi.TruckPatrol.Controllers
{
    public class RFIDUploadController : ApiController
    {
        //[HttpPost]
        //public HttpResponseMessage Sync()
        //{
        //    HttpResponseMessage response = new HttpResponseMessage();

        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;

        //        if (httpRequest.Form.Count != 0)
        //        {
        //            var postedFile = httpRequest.Files[0];

        //            var guid = Guid.NewGuid().ToString();

        //            var folder = Path.Combine(Utility.Config.TruckCheckRFIDUploadFolderPath, guid);

        //            Directory.CreateDirectory(folder);

        //            postedFile.SaveAs(Path.Combine(folder, "EquipCheck.RFID.Upload.zip"));

        //            RequestResult result = RFIDUploadHelper.Upload(guid);

        //            if (result.IsSuccess)
        //            {
        //                response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Message = result.Message });
        //            }
        //            else
        //            {
        //                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Error.ErrorMessage });
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("無資料可接收");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { Success = false, Message = err.ErrorMessage });
        //    }

        //    return response;
        //}
    }
}