using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.IslandManagement
{
    public class FormInput
    {
        [DisplayName("裝/卸料站")]
        public string StationUniqueID { get; set; }

        [DisplayName("灌島代號")]
        [Required(ErrorMessage = "請輸入灌島代號")]
        public string ID { get; set; }

        [DisplayName("灌島描述")]
        [Required(ErrorMessage = "請輸入灌島描述")]
        public string Description { get; set; }
    }
}
