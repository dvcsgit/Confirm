using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class DownloadFormModel
    {
        public string CheckDate { get; set; }

        public List<DownloadParameters> Parameters { get; set; }

        public List<string> JobUniqueIDList
        {
            get
            {
                return Parameters.Where(x => !string.IsNullOrEmpty(x.JobUniqueID)).Select(x => x.JobUniqueID).Distinct().ToList();
            }
        }

        public DownloadFormModel()
        {
            Parameters = new List<DownloadParameters>();
        }
    }
}