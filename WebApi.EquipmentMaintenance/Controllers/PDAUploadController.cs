using DataSync.EquipmentMaintenance;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.EquipmentMaintenance.Controllers
{
    public class PDAUploadController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Sync()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var task = this.Request.Content.ReadAsStreamAsync();

                task.Wait();

                var guid = Guid.NewGuid().ToString();

                var folder = Path.Combine(Config.EquipmentMaintenanceSQLiteUploadFolderPath, guid);

                Directory.CreateDirectory(folder);

                using (Stream fs = File.Create(Path.Combine(folder, "EquipmentMaintenance.Upload.zip")))
                {
                    task.Result.CopyTo(fs);

                    fs.Close();

                    task.Result.Close();
                }

                RequestResult result = UploadHelper.Upload(guid, null);

                if (result.IsSuccess)
                {
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Message = result.Message });
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Error.ErrorMessage });
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