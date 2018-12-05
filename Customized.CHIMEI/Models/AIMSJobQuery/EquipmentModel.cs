using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class EquipmentModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public bool HaveAbnormal
        {
            get
            {
                return CheckItemList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return CheckItemList.Any(x => x.HaveAlert);
            }
        }

        public double CheckItemCount
        {
            get
            {
                return CheckItemList.Count;
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return CheckItemList.Count(x => x.IsChecked);
            }
        }

        public string CompleteRate
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return "-";
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        return "0%";
                    }
                    else
                    {

                        if (CheckItemCount == CheckedItemCount)
                        {
                            return "100%";
                        }
                        else
                        {
                            return (CheckedItemCount / CheckItemCount).ToString("#0.00%");
                        }
                    }
                }
            }
        }

        public List<UserModel> CheckUserList
        {
            get
            {
                return ArriveRecordList.Select(x => x.ArriveUser).ToList();
            }
        }

        public List<ArriveRecordModel> ArriveRecordList { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public EquipmentModel()
        {
            ArriveRecordList = new List<ArriveRecordModel>();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
