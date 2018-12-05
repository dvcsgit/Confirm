using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class PipelineSpecModel
    {
        public string Spec { get; set; }

        public int Seq { get; set; }

        public string SpecType { get; set; }

        public string Option { get; set; }

        public string Input { get; set; }

        public string Value
        {
            get
            {
                if (!string.IsNullOrEmpty(Option))
                {
                    return Option;
                }
                else
                {
                    return Input;
                }
            }
        }
    }
}
