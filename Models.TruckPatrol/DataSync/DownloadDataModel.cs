using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.DataSync
{
    public class DownloadDataModel
    {
        public List<TruckModel> TruckList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return TruckList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<AbnormalReasonModel> AbnormalReasonList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.AbnormalReasonList).Distinct().ToList();
            }
        }

        public List<HandlingMethodModel> HandlingMethodList
        {
            get
            {
                return AbnormalReasonList.SelectMany(x => x.HandlingMethodList).Distinct().ToList();
            }
        }

        public List<PrevCheckResultModel> PrevCheckResultList { get; set; }

        public List<UnRFIDReason> UnRFIDReasonList { get; set; }

        public List<UserModel> UserList { get; set; }

        public Dictionary<string, string> LastModifyTimeList
        {
            get
            {
                return TruckList.ToDictionary(x => x.UniqueID, x => x.LastModifyTime);
            }
        }

        public DownloadDataModel()
        {
            TruckList = new List<TruckModel>();
            UnRFIDReasonList = new List<UnRFIDReason>();
            PrevCheckResultList = new List<PrevCheckResultModel>();
        }
    }
}
