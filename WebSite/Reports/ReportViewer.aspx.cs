using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utility;

namespace WebSite.Reports
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GenerateReport();
            }
        }

        public void GenerateReport()
        {
            var ReportWrapperSessionKey = "ReportWrapper";

            var rw = (ReportWrapper)Session[ReportWrapperSessionKey];

            if (rw != null)
            {
                try
                {
                    RptViewer.LocalReport.ReportPath = rw.ReportPath;

                    RptViewer.LocalReport.DataSources.Clear();

                    foreach (var reportDataSource in rw.ReportDataSources)
                    {
                        RptViewer.LocalReport.DataSources.Add(reportDataSource);
                    }

                    RptViewer.LocalReport.SetParameters(rw.ReportParameters);

                    RptViewer.LocalReport.Refresh();

                    if (rw.IsDownloadDirectly)
                    {
                        Warning[] warnings;
                        string[] streamids;
                        string mimeType;
                        string encoding;
                        string extension;

                        byte[] bytes = RptViewer.LocalReport.Render(
                        rw.DownloadType, null, out mimeType, out encoding, out extension,
                        out streamids, out warnings);

                        Response.Clear();
                        Response.ContentType = mimeType;
                        Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.{1}", rw.FileName, extension));
                        Response.BinaryWrite(bytes);

                        Session[ReportWrapperSessionKey] = null;

                        Response.End();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(MethodBase.GetCurrentMethod(), ex);

                    Response.End();
                }
            }
        }
    }
}