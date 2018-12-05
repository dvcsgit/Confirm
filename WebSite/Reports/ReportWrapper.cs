using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSite.Reports
{
    public class ReportWrapper
    {
        // Constructors
        public ReportWrapper()
        {
            ReportDataSources = new List<ReportDataSource>();
            ReportParameters = new List<ReportParameter>();
        }

        public string FileName { get; set; }

        public string ReportPath { get; set; }

        public List<ReportDataSource> ReportDataSources { get; set; }

        public List<ReportParameter> ReportParameters { get; set; }

        public bool IsDownloadDirectly { get; set; }

        //下載 Excel 或 PDF
        public string DownloadType { get; set; }
    }
}