using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.DataSync
{
    public class DownloadFormModel
    {
        public string CheckDate { get; set; }

        public List<DownloadParameters> Parameters { get; set; }

        public DownloadFormModel()
        {
            Parameters = new List<DownloadParameters>();
        }
    }
}
