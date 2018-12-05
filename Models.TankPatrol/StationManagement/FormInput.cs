using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.StationManagement
{
    public class FormInput
    {
        [DisplayName("裝/卸料站代號")]
        [Required(ErrorMessage = "請輸入裝/卸料站代號")]
        public string ID { get; set; }

        [DisplayName("裝/卸料站描述")]
        [Required(ErrorMessage = "請輸入裝/卸料站描述")]
        public string Description { get; set; }
    }
}
