using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.DataSync
{
    public class RemarkModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public override bool Equals(object Object)
        {
            return Equals(Object as RemarkModel);
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(RemarkModel Model)
        {
            return UniqueID.Equals(Model.UniqueID);
        }
    }
}
