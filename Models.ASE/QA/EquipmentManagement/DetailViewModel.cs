using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public EquipmentModel Equipment { get; set; }

        public Models.ASE.QA.CalibrationApply.GridItem CalibrationApply { get; set; }

        public List<Models.ASE.QA.CalibrationNotify.GridItem> CalibrationNotifyList { get; set; }

        public List<Models.ASE.QA.CalibrationForm.GridItem> CalibrationFormList { get; set; }

        public List<Models.ASE.QA.MSANotify.GridItem> MSANotifyList { get; set; }

        public List<Models.ASE.QA.MSAForm_v2.GridItem> MSAFormList { get; set; }

        public List<Models.ASE.QA.ChangeForm.GridItem> ChangeFormList { get; set; }

        public List<Models.ASE.QA.AbnormalForm.GridItem> AbnormalFormList { get; set; }

        //public bool CanCAL
        //{
        //    get
        //    {
        //        bool canCal = true;

        //        if (CalibrationApply != null && CalibrationApply.Status.Status != "3" && CalibrationApply.Status.Status != "4")
        //        {
        //            canCal = false;
        //        }
        //    }
        //}

        //public bool CanMSA
        //{
        //    get
        //    {
        //    }
        //}
    }
}
