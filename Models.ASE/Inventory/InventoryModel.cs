using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.Inventory
{
    public class InventoryModel
    {
        public DateTime AlterTime { get; set; }

        public string AlterTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(AlterTime);
            }
        }

        public string Type { get; set; }

        public string TypeDescription
        {
            get
            {
                if (Type == "In")
                {
                return "入庫";
                }
                else if (Type == "Out")
                {
                    return "領料";
                }
                else if (Type == "C")
                {
                    return "盤點";
                }
                else
                {
                    return "-";
                }
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public int? Quantity { get; set; }
    }
}
