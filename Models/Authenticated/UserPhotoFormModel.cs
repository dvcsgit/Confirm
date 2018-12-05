using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utility.Models;

namespace Models.Authenticated
{
    public class UserPhotoFormModel
    {
        public RequestResult RequestResult { get; set; }

        public HttpPostedFileBase Photo { get; set; }
    }
}
