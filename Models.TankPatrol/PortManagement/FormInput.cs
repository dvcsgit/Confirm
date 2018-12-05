using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.PortManagement
{
    public class FormInput
    {
        [DisplayName("裝/卸料站")]
        public string StationUniqueID { get; set; }

        [DisplayName("灌島")]
        public string IslandUniqueID { get; set; }

        [DisplayName("灌口代號")]
        [Required(ErrorMessage = "請輸入灌口代號")]
        public string ID { get; set; }

        [DisplayName("灌口描述")]
        [Required(ErrorMessage = "請輸入灌口描述")]
        public string Description { get; set; }

        [DisplayName("TagID")]
        public string TagID { get; set; }

        public int? LB2LPTimeSpan { get; set; }

        public int? LP2LATimeSpan { get; set; }

        public int? LA2LDTimeSpan { get; set; }

        public int? UB2UPTimeSpan { get; set; }

        public int? UP2UATimeSpan { get; set; }

        public int? UA2UDTimeSpan { get; set; }
    }
}
