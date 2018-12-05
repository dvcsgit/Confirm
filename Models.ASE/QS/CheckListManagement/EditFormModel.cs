using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.CheckListManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }


        public string VHNO { get; set; }
        public string Factory { get; set; }

        //public List<FactoryModel> FactoryList { get; set; }

        public List<ShiftModel> ShiftList { get; set; }

        public List<StationModel> StationList { get; set; }

        public List<string> FormStationList { get; set; }

        public List<CheckTypeModel> CheckTypeList { get; set; }

        public List<ResDepartmentModel> ResDepartmentList { get; set; }

        public List<AuditTypeModel> AuditTypeList { get; set; }

        public List<RiskModel> RiskList { get; set; }

        public List<GradeModel> GradeList { get; set; }

        public FormInput FormInput { get; set; }

        public List<PhotoModel> PhotoList
        {
            get
            {
                return CheckTypeList.SelectMany(x => x.PhotoList).ToList();
            }
        }

        public EditFormModel()
        {
            //FactoryList = new List<FactoryModel>();
            ShiftList = new List<ShiftModel>();
            StationList = new List<StationModel>();
            FormStationList = new List<string>();
            CheckTypeList = new List<CheckTypeModel>();
            ResDepartmentList = new List<ResDepartmentModel>();
            AuditTypeList = new List<AuditTypeModel>();
            RiskList = new List<RiskModel>();
            GradeList = new List<GradeModel>();
            FormInput = new FormInput();
        }
    }
}
