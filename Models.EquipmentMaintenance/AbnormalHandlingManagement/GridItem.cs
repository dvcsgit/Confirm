using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.AbnormalHandlingManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string Type
        {
            get
            {
                if (!string.IsNullOrEmpty(CheckItemID))
                {
                    return "P";
                }
                else if (!string.IsNullOrEmpty(StandardID))
                {
                    return "M";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string TypeDisplay
        {
            get
            {
                if (Type == "P")
                {
                    return "巡檢";
                }
                else if (Type == "M")
                {
                    return "預防保養";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string ControlPointDisplay
        {
            get
            {
                return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
            }
        }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string EquipmentDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public string Display
        {
            get
            {
                if (string.IsNullOrEmpty(EquipmentID))
                {
                    return ControlPointDisplay;
                }
                else
                {
                    return EquipmentDisplay;
                }
            }
        }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItemDisplay
        {
            get
            {
                return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
            }
        }

        public string StandardID { get; set; }

        public string StandardDescription { get; set; }

        public string StandardDisplay
        {
            get
            {
                return string.Format("{0}/{1}", StandardID, StandardDescription);
            }
        }

        public string ItemDisplay
        {
            get
            {
                if (Type == "M")
                {
                    return StandardDisplay;
                }
                else if (Type == "P")
                {
                    return CheckItemDisplay;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Date { get; set; }

        public string Time { get; set; }

        public string CheckTimeDisplay
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(Date, Time));
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public UserModel CheckUser { get; set; }

        public DateTime? ClosedTime { get; set; }

        public bool IsClosed
        {
            get
            {
                return ClosedTime.HasValue;
            }
        }

        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public UserModel ClosedUser { get; set; }

        public List<UserModel> ChargePersonList { get; set; }

        public List<string> ChargePersonIDList
        {
            get
            {
                return ChargePersonList.Select(x => x.ID).ToList();
            }
        }

        public string ChargePersons
        {
            get
            {
                if (ChargePersonList != null && ChargePersonList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var chargePerson in ChargePersonList)
                    {
                        sb.Append(chargePerson.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public GridItem()
        {
            ClosedUser = new UserModel();
            CheckUser = new UserModel();
            ChargePersonList = new List<UserModel>();
        }
    }
}
