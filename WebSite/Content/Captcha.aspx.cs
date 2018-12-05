using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utility;

namespace WebSite.Content
{
    public partial class Captcha : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(70, 30);

            Graphics graphics = Graphics.FromImage(bmp);
            graphics.Clear(Color.Black);

            Font font = new Font("Courier New", 14, FontStyle.Bold);

            Session["Captcha"] = RandomPassword.Generate(5, 5);

            graphics.DrawString(Convert.ToString(Session["Captcha"]),
                font, Brushes.White, 3, 3);

            Response.ContentType = "image/gif";
            bmp.Save(Response.OutputStream, ImageFormat.Gif);

            font.Dispose();
            graphics.Dispose();
            bmp.Dispose();

            Response.End();
        }
    }
}