using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAReport
{
   public class AnovaGridItem
    {
       public DateTime? MSADate { get; set; }

        public string MSADateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(MSADate);
            }
        }

        public int Cycle { get; set; }

        public string DueDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(MSADate.Value.AddMonths(Cycle).AddDays(-1));
            }
        }

        public string MSACalNo { get; set; }

        public string MSAIchi { get; set; }

        public string MSACharacteristic { get; set; }

        public string Model { get; set; }

        public string SerialNo { get; set; }

        public string Brand { get; set; }

        public string Factory { get; set; }

        public string MSAResponsorID { get; set; }

        public string MSAResponsorName { get; set; }

        public string MSAResponsor
        {
            get
            {
                if (!string.IsNullOrEmpty(MSAResponsorName))
                {
                    return string.Format("{0}/{1}", MSAResponsorID, MSAResponsorName);
                }
                else
                {
                    return MSAResponsorID;
                }
            }
        }

        public string ManagerID { get; set; }

        public string ManagerName { get; set; }

        public string Manager
        {
            get
            {
                if (!string.IsNullOrEmpty(ManagerName))
                {
                    return string.Format("{0}/{1}", ManagerID, ManagerName);
                }
                else
                {
                    return ManagerID;
                }
            }
        }

        public StabilityResult StabilityResult { get; set; }

        public BiasResult BiasResult { get; set; }

        public LinearityResult LinearityResult { get; set; }

        public GRRResult GRRHResult { get; set; }
        public GRRResult GRRMResult { get; set; }
        public GRRResult GRRLResult { get; set; }

        public AnovaResult AnovaHResult { get; set; }
        public AnovaResult AnovaMResult { get; set; }
        public AnovaResult AnovaLResult { get; set; }

        public AnovaGridItem()
        {
            GRRHResult = new GRRResult();
            GRRMResult = new GRRResult();
            GRRLResult = new GRRResult();

            AnovaHResult = new AnovaResult();
            AnovaMResult = new AnovaResult();
            AnovaLResult = new AnovaResult();

            StabilityResult = new StabilityResult();
            BiasResult = new BiasResult();
            LinearityResult = new LinearityResult();
            
        }
    }
}
