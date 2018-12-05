using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.AbnormalHanding
{
    public class ArriveRecordModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public List<string> UserList { get; set; }

        public string Users
        {
            get
            {
                var sb = new StringBuilder();

                if (UserList.Count > 0)
                {
                    foreach (var user in UserList)
                    {
                        sb.Append(user);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);
                }
                else
                {
                    sb.Append("-");
                }

                return sb.ToString();
            }
        }
        public ArriveRecordModel()
        {
            UserList = new List<string>();
        }
    }
}
