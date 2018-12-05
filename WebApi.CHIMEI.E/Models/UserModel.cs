using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class UserModel
    {
        public string ID { get; set; }

        public string Title { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string UID { get; set; }

        public override bool Equals(object Object)
        {
            return Equals(Object as UserModel);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public bool Equals(UserModel Model)
        {
            return ID.Equals(Model.ID);
        }
    }
}