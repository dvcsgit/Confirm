using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Utility;
using Utility.Models;

namespace WebApi.EquipmentMaintenance.Controllers
{
    public class TagUploadController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Sync()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count != 0)
                {
                    var postedFile = httpRequest.Files[0];

                    var guid = Guid.NewGuid().ToString();

                    var folder = Path.Combine(Config.EquipmentMaintenanceTagSQLiteUploadFolderPath, guid);

                    Directory.CreateDirectory(folder);

                    postedFile.SaveAs(Path.Combine(folder, "EquipmentMaintenance.Tag.Upload.zip"));

                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true });
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