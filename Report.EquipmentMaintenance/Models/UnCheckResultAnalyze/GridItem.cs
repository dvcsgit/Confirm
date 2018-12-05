using Utility;
using System;
using System.Collections.Generic;
using System.Text;
namespace Report.EquipmentMaintenance.Models.UnCheckResultAnalyze
{
    public class GridItem
    {
        public string Route
        {
            get
            {
                if (!string.IsNullOrEmpty(JobDescription))
                {
                    if (!string.IsNullOrEmpty(RouteName))
                    {
                        return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
                    }
                    else
                    {
                        return string.Format("{0}-{1}", RouteID, JobDescription);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(RouteName))
                    {
                        return string.Format("{0}/{1}", RouteID, RouteName);
                    }
                    else
                    {
                        return RouteID;
                    }
                }
            }
        }
        

        public string ControlPoint
        {
            get
            {
                if (!string.IsNullOrEmpty(ControlPointDescription))
                {
                    return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
                }
                else
                {
                    return ControlPointID;
                }
            }
        }

        public string Equipment
        {
            get
            {
                if (string.IsNullOrEmpty(EquipmentName))
                {
                    return string.Format("{0}", EquipmentID);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public string CheckItem
        {
            get
            {
                if (!string.IsNullOrEmpty(CheckItemDescription))
                {
                    return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
                }
                else
                {
                    return CheckItemID;
                }
            }
        }


        public string ArriveDateTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(ArriveDate);
            }
        }

        public string User
        {
            get
            {
                if (UserList!=null&&UserList.Count!=0)
                {
                    StringBuilder builder=new StringBuilder();
                    foreach (var userItem in UserList)
                    {
                        builder.Append(userItem.UserID + "/" + userItem.UserName+",");
                    }
                    return builder.ToString();
                }
                else
                {
                    return "";
                }
            }
        }

        public string JobDescription { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public DateTime?  ArriveDate { get; set; }

        public List<UserModel> UserList { get; set; }

    }
}
