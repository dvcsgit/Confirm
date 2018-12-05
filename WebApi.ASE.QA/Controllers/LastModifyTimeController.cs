using System.Web.Http;
using DataAccess.ASE.QA;

namespace WebApi.ASE.QA.Controllers
{
    public class LastModifyTimeController : ApiController
    {
        [HttpGet]
        public string Get(string JobUniqueID)
        {
            return LastModifyTimeHelper.Get(JobUniqueID);
        }
    }
}