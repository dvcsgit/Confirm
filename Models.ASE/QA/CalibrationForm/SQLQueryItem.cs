using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationForm
{
    public class SQLQueryItem
    {
        public string UniqueID { get; set; }
        public string VHNO { get; set; }
        public string Status { get; set; }
        public string OrganizationUniqueID { get; set; }
        public string OrganizationDescription { get; set; }
        public DateTime EstCalibrateDate { get; set; }
        public DateTime? CalibrateDate { get; set; }
        public string ResponsorID { get; set; }
        public string ResponsorName { get; set; }
        public string CalibratorID { get; set; }
        public string CalibratorName { get; set; }
        public string LabDescription { get; set; }
        public DateTime NotifyTime { get; set; }
        public DateTime? TakeJobTime { get; set; }
        public string OwnerID { get; set; }
        public string OwnerManagerID { get; set; }
        public string PEID { get; set; }
        public string PEManagerID { get; set; }
        public string JobCalibratorID { get; set; }
        public string JobCalibratorName { get; set; }
        public string CalNo { get; set; }
        public string SerialNo { get; set; }
        public string IchiUniqueID { get; set; }
        public string IchiName { get; set; }
        public string IchiRemark { get; set; }
        public string MachineNo { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string CalibrateType { get; set; }
        public string CalibrateUnit { get; set; }
        public string IsQRCoded { get; set; }
        public int? LogSeq { get; set; }
        public string Step { get; set; }
    }
}
