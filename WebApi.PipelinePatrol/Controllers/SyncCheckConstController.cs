using DataSync.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
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
using Models.PipelinePatrol.FCM;

namespace WebApi.PipelinePatrol.Controllers
{
    /// <summary>
    /// 同步檢查
    /// TODO 之後要抽離
    /// </summary>
    public class SyncCheckConstController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">PipelineAbnormal\Construction\Preview</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                Logger.Log(MethodBase.GetCurrentMethod());
                
                RequestResult result = CheckDataSyncHelper.GetVHNOList(); //ConstructionHelper.();
                

                if (result.IsSuccess)
                {
                    response = Request.CreateResponse(HttpStatusCode.OK, new { IsSuccess = true, Data = result.Data });
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, Message = err.ErrorMessage });
            }

            return response;
        }

        
    }


}