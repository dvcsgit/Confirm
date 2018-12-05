using DataAccess.ASE.QA;
using Models.ASE.QA.DataSync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.ASE.QA.Controllers
{
    public class EquipmentPhotoController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string CALNO)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                RequestResult result = EquipmentHelper.GetPhoto(CALNO);

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

        [HttpPost]
        public HttpResponseMessage Create()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Form.Count != 0)
                {
                    string jsonStringValue = httpRequest.Form[0];

                    Logger.Log(jsonStringValue);

                    var formInput = JsonConvert.DeserializeObject<EquipmentPhotoFormInput>(jsonStringValue);
                    //處理附檔
                    var postedFile = httpRequest.Files[0];

                    var guid = Guid.NewGuid().ToString();

                    var folder = Path.Combine(Config.TempFolder, guid);

                    Directory.CreateDirectory(folder);

                    postedFile.SaveAs(Path.Combine(folder, "EquipmentPhoto.zip"));

                    RequestResult result = EquipmentHelper.Create(folder, formInput.CALNO);

                    if (result.IsSuccess)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true });
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