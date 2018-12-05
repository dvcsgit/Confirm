using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.ResultQuery
{
    public class UserLocus
    {
        public UserModel User { get; set; }

        public List<UserLocation> Locus { get; set; }

        public UserLocus()
        {
            User = new UserModel();
            Locus = new List<UserLocation>();
        }
    }
}
