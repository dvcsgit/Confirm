﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.DataSync
{
    public class StandardModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItem { get; set; }

        public decimal? LowerLimit { get; set; }

        public decimal? LowerAlertLimit { get; set; }

        public decimal? UpperAlertLimit { get; set; }

        public decimal? UpperLimit { get; set; }

        public string Unit { get; set; }

        public string Remark { get; set; }

        public int Seq { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public List<FeelOptionModel> FeelOptionList { get; set; }

        public StandardModel()
        {
            AbnormalReasonList = new List<AbnormalReasonModel>();
            FeelOptionList = new List<FeelOptionModel>();
        }

        public override bool Equals(object Object)
        {
            return Equals(Object as StandardModel);
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(StandardModel Model)
        {
            return UniqueID.Equals(Model.UniqueID);
        }
    }
}
