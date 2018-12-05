using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Models.PipelinePatrol.Shared;

namespace WebApi.PipelinePatrol
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //http://blog.darkthread.net/post-2013-10-03-jsonnet-datetimezonehandling.aspx
            //讓拋回去前端的資料 格式一致
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling
                = Newtonsoft.Json.DateTimeZoneHandling.Unspecified;

            config.Formatters.JsonFormatter.SerializerSettings.DateFormatString = DateFormateConsts.UI_S_yyyyMMddhhmmss;

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // 取消註解以下程式碼行以啟用透過 IQueryable 或 IQueryable<T> 傳回類型的動作查詢支援。
            // 為了避免處理未預期或惡意佇列，請使用 QueryableAttribute 中的驗證設定來驗證傳入的查詢。
            // 如需詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkId=279712。
            //config.EnableQuerySupport();

            // 若要停用您應用程式中的追蹤，請將下列程式碼行標記為註解或加以移除
            // 如需詳細資訊，請參閱: http://www.asp.net/web-api
            config.EnableSystemDiagnosticsTracing();
        }
    }
}
