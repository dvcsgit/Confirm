using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.Shared
{
    public class MoveToTarget
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public Define.EnumMoveDirection Direction { get; set; }
    }
}
