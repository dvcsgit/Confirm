using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.DailyReport
{
    public class JobModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        //public List<string> UserList { get; set; }

        //public string Users
        //{
        //    get
        //    {
        //        var sb = new StringBuilder();

        //        if (UserList.Count > 0)
        //        {
        //            foreach (var user in UserList)
        //            {
        //                sb.Append(user);
        //                sb.Append("、");
        //            }

        //            sb.Remove(sb.Length - 1, 1);
        //        }
        //        else
        //        {
        //            sb.Append("-");
        //        }

        //        return sb.ToString();
        //    }
        //}

        public string Users { get; set; }

        //public JobModel()
        //{
        //    UserList = new List<string>();
        //}
    }
}
