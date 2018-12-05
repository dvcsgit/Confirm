using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class CountReferenceValue
    {
        public int Sample { get; set; }

        public int? Reference { get; set; }

        public int? ReferenceValue { get; set; }
    }
}
