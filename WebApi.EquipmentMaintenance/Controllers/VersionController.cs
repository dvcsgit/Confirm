using System.Web.Http;
using Utility;
using DataSync.EquipmentMaintenance;

namespace WebApi.EquipmentMaintenance.Controllers
{
    public class VersionController : ApiController
    {
        public string Get(string AppName, Define.EnumDevice Device)
        {
            return VersionHelper.Get(AppName, Device);
        }
    }
}