using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Utility;
using Models.Shared;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class JobModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public List<UserModel> JobUserList { get; set; }

        public bool HaveAbnormal
        {
            get
            {
                return EquipmentList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return EquipmentList.Any(x => x.HaveAlert);
            }
        }

        public double CheckItemCount
        {
            get
            {
                return EquipmentList.Sum(x => x.CheckItemCount);
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return EquipmentList.Sum(x => x.CheckedItemCount);
            }
        }

        public List<UserModel> CheckUserList
        {
            get
            {
                return EquipmentList.SelectMany(x => x.CheckUserList).Distinct().ToList();
            }
        }

        public List<EquipmentModel> EquipmentList { get; set; }

        public JobModel()
        {
            EquipmentList = new List<EquipmentModel>();
            JobUserList = new List<UserModel>();
        }
    }
}
