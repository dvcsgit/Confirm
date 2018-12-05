using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.DataSync
{
    public class Version
    {
        public int Id { get; set; }
        public string AppName { get; set; }
        public string ApkName { get; set; }
        public string VerName { get; set; }
        public int VerCode { get; set; }
        public string ReleaseNote { get; set; }
        public bool IsForceUpdate { get; set; }
        public DateTime DateReleased { get; set; }
        public DateTime DateCreated { get; set; }
        public string Device { get; set; }
    }
}
