﻿using System.Linq;
using System.Collections.Generic;

namespace Models.TankPatrol.DataSync
{
    public class CheckItemModel
    {
        public string UniqueID { get; set; }

        public string TagID { get; set; }

        public string CheckType { get; set; }

        public string Procedure { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItem { get; set; }

        public int? TextValueType { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public List<FeelOptionModel> FeelOptionList { get; set; }

        public CheckItemModel()
        {
            AbnormalReasonList = new List<AbnormalReasonModel>();
            FeelOptionList = new List<FeelOptionModel>();
        }

        public override bool Equals(object Object)
        {
            return Equals(Object as CheckItemModel);
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(CheckItemModel Model)
        {
            return UniqueID.Equals(Model.UniqueID);
        }
    }
}
