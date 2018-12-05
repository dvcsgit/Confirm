using System.Linq;
using System.Collections.Generic;

namespace Models.TankPatrol.DataSync
{
    public class DownloadDataModel
    {
        public List<StationModel> StationList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return StationList.SelectMany(x => x.CheckItemList).Distinct().ToList();
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

        public List<UnRFIDReason> UnRFIDReasonList { get; set; }

        public List<OptionModel> OptionList { get; set; }

        public List<OptionModel> DriverList
        {
            get
            {
                return OptionList.Where(x => x.Type == "D").ToList();
            }
        }

        public List<OptionModel> OwnerList
        {
            get
            {
                return OptionList.Where(x => x.Type == "O").ToList();
            }
        }

        public List<OptionModel> TankList
        {
            get
            {
                return OptionList.Where(x => x.Type == "C").ToList();
            }
        }

        public List<OptionModel> MaterialList
        {
            get
            {
                return OptionList.Where(x => x.Type == "M").ToList();
            }
        }

        public List<UserModel> UserList { get; set; }

        public DownloadDataModel()
        {
            StationList = new List<StationModel>();
            UnRFIDReasonList = new List<UnRFIDReason>();
            UserList = new List<UserModel>();
            OptionList = new List<OptionModel>();
        }
    }
}
