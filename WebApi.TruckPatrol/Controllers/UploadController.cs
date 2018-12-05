using DataSync.TruckPatrol;
using Models.TruckPatrol.DataSync;
using Newtonsoft.Json;
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
    public class UploadController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Sync()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Form.Count != 0)
                {
                    string jsonStringValue = httpRequest.Form[0];

                    var formInput = JsonConvert.DeserializeObject<ApiFormInput>(jsonStringValue);
                    //處理附檔
                    var postedFile = httpRequest.Files[0];

                    var guid = Guid.NewGuid().ToString();

                    var folder = Path.Combine(Config.TruckPatrolSQLiteUploadFolderPath, guid);

                    Directory.CreateDirectory(folder);

                    postedFile.SaveAs(Path.Combine(folder, "TruckPatrol.Upload.zip"));

                    RequestResult result = UploadHelper.Upload(guid, formInput.UserID);

                    if (result.IsSuccess)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Message = result.Message });
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Error.ErrorMessage });
                    }
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

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { Success = false, Message = err.ErrorMessage });
            }

            return response;
        }
    }
}