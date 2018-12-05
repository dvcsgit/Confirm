using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.ASE.DataSync;
using DataAccess.ASE.QA;

namespace WebApi.ASE.QA.Controllers
{
    public class UploadController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Sync()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log("Upload.Sync");

                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Form.Count != 0)
                {
                    string jsonStringValue = httpRequest.Form[0];

                    Logger.Log(jsonStringValue);

                    var formInput = JsonConvert.DeserializeObject<ApiFormInput>(jsonStringValue);
                    //處理附檔
                    var postedFile = httpRequest.Files[0];

                    var guid = Guid.NewGuid().ToString();

                    var folder = Path.Combine(Config.QASQLiteUploadFolderPath, guid);

                    Directory.CreateDirectory(folder);

                    postedFile.SaveAs(Path.Combine(folder, "QA.Upload.zip"));

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