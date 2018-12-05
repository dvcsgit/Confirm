using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        //[Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        //public string VHNO { get; set; }

        //public FormStatus Status { get; set; }

        public string Type { get; set; }

        //public string SubType { get; set; }

        //public string TypeDisplay
        //{
        //    get
        //    {
        //        if (Type == "1")
        //        {
        //            if (SubType == "1")
        //            {
        //                return "計量(全距平均法)";
        //            }
        //            else if (SubType == "2")
        //            {
        //                return "計量(ANOVA)";
        //            }
        //            else
        //            {
        //                return "計量";
        //            }
        //        }
        //        else if (Type == "2")
        //        {
        //            return "計數";
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //}

        //public DateTime EstMSADate { get; set; }

        //[Display(Name = "MSADate", ResourceType = typeof(Resources.Resource))]
        //public string EstMSADateString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateStringWithSeperator(EstMSADate);
        //    }
        //}

        //public DateTime CreateTime { get; set; }

        //[Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        //public string CreateTimeString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
        //    }
        //}

        //public string MSAResponsorID { get; set; }

        //public string MSAResponsorName { get; set; }

        //[Display(Name = "MSAResponsor", ResourceType = typeof(Resources.Resource))]
        //public string MSAResponsor
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(MSAResponsorName))
        //        {
        //            return string.Format("{0}/{1}", MSAResponsorID, MSAResponsorName);
        //        }
        //        else
        //        {
        //            return MSAResponsorID;
        //        }
        //    }
        //}

        //[Display(Name = "Station", ResourceType = typeof(Resources.Resource))]
        //public string Station { get; set; }

        //public string MSAIchi { get; set; }

        //public string MSALowerRange { get; set; }

        //public string MSAUpperRange { get; set; }

        //[Display(Name = "MSARange", ResourceType = typeof(Resources.Resource))]
        //public string MSARange
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(MSALowerRange) && !string.IsNullOrEmpty(MSAUpperRange))
        //        {
        //            return string.Format("{0}~{1}", MSALowerRange, MSAUpperRange);
        //        }
        //        else if (!string.IsNullOrEmpty(MSALowerRange) && string.IsNullOrEmpty(MSAUpperRange))
        //        {
        //            return string.Format(">{0}", MSALowerRange);
        //        }
        //        else if (string.IsNullOrEmpty(MSALowerRange) && !string.IsNullOrEmpty(MSAUpperRange))
        //        {
        //            return string.Format("<{0}", MSAUpperRange);
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //}

        //[Display(Name = "Characteristic", ResourceType = typeof(Resources.Resource))]
        //public string MSACharacteristic { get; set; }

        //public string Unit { get; set; }

        //public EquipmentModel Equipment { get; set; }

        //public List<StabilityItem> StabilityList { get; set; }

        //public StabilityResult StabilityResult { get; set; }

        //public List<BiasItem> BiasList { get; set; }

        //public BiasResult BiasResult { get; set; }

        //public List<LinearityItem> LinearityList { get; set; }

        //public LinearityResult LinearityResult { get; set; }

        //public List<GRRItem> GRRList { get; set; }

        //public GRRResult GRRHResult { get; set; }
        //public GRRResult GRRMResult { get; set; }
        //public GRRResult GRRLResult { get; set; }

        //public AnovaResult AnovaHResult { get; set; }
        //public AnovaResult AnovaMResult { get; set; }
        //public AnovaResult AnovaLResult { get; set; }

        //public List<CountReferenceValue> CountReferenceValueList { get; set; }
        //public List<CountItem> CountList { get; set; }

        //public CountResult CountResult { get; set; }

        public FormInput FormInput { get; set; }

        //public List<LogModel> LogList { get; set; }

        public EditFormModel()
        {
            //LogList = new List<LogModel>();
            //Equipment = new EquipmentModel();
            //StabilityList = new List<StabilityItem>();
            //StabilityResult = new StabilityResult();
            //BiasList = new List<BiasItem>();
            //BiasResult = new BiasResult();
            //LinearityList = new List<LinearityItem>();
            //LinearityResult = new LinearityResult();
            //GRRList = new List<GRRItem>();
            //GRRHResult = new GRRResult();
            //GRRMResult = new GRRResult();
            //GRRLResult = new GRRResult();
            //AnovaHResult = new AnovaResult();
            //AnovaMResult = new AnovaResult();
            //AnovaLResult = new AnovaResult();
            //CountReferenceValueList = new List<CountReferenceValue>();
            //CountList = new List<CountItem>();
            //CountResult = new CountResult();
        }
    }
}
