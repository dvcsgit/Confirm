#if ASE
using DbEntity.ASE;
#else
using DbEntity.MSSQL;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Reflection;
using System.Net; 

namespace Portal.Controllers
{
    public class HomeController : Controller
    {
        private string LoginFile = @"C:\FEM\WindowsAuthentication.xml";

#if ASE
        private string LoginFile = @"D:\FEM\WindowsAuthentication.xml";
        private string IE10ConfigFileName = "IE10.xml";

        private string ClientComputerName
        {
            get
            {
                string clientIP = Request.UserHostAddress;

                try
                {
                    IPHostEntry hostInfo = Dns.GetHostEntry(clientIP);
                    return hostInfo.HostName; 
                }
                catch
                {
                    return clientIP;
                }
                
            }
        }

        private string IE10ConfigFile
        {
            get
            {
                string exePath = System.AppDomain.CurrentDomain.BaseDirectory;

                var filePath = Path.Combine(exePath, IE10ConfigFileName);

                if (System.IO.File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = Path.Combine(exePath, "bin", IE10ConfigFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        return IE10ConfigFileName;
                    }
                }
            }
        }
#endif

        public ActionResult Index(string ReturnUrl)
        {
            return RedirectToAction("WindowsAuthentication", new { ReturnUrl = ReturnUrl });
        }

#if ASE
        public ActionResult IE10()
        {
            Logger.Log("IE10");

            return View();
        }

        public ActionResult IE10Download(string Lang)
        {
            var root = XDocument.Load(IE10ConfigFile).Root;

            if (Lang == "en-us")
            {
                return File(root.Element("IE10").Attribute("en_us").Value, "application/exe", "IE10_en_us.exe");
            }
            else
            {
                return File(root.Element("IE10").Attribute("zh_tw").Value, "application/exe", "IE10_zh_tw.exe");
            }
        }

        public ActionResult Compatibility()
        {
            Logger.Log("Compatibility");

            var root = XDocument.Load(IE10ConfigFile).Root;

            var elementList = root.Elements("RECORD").ToList();

            var machineName = Request.UserHostName;

            if (elementList.Any(x => x.Value == machineName))
            {
                return RedirectToAction("WindowsAuthentication");
            }
            else
            {
                return View();
            }
        }

        public ActionResult CompatibilityDownload()
        {
            var doc = XDocument.Load(IE10ConfigFile);

            var machineName = Request.UserHostName;

            doc.Root.Add(new XElement("RECORD", machineName));

            doc.Save(IE10ConfigFile);

            return File(doc.Root.Element("Compatibility").Value, "application/exe", "IE10_Compatibility.exe");
        }
#endif

        public ActionResult WindowsAuthentication(string ReturnUrl)
        {
            //try
            //{
            //    var doc = XDocument.Load(IE10ConfigFile);

            //    var elementList = doc.Root.Elements("RECORD").ToList();

            //    var machineName = Request.UserHostName;

            //    if (!elementList.Any(x => x.Value == machineName))
            //    {
            //        doc.Root.Add(new XElement("RECORD", machineName));
            //    }

            //    doc.Save(IE10ConfigFile); 
            //}
            //catch (Exception ex)
            //{
            //    Logger.Log(MethodBase.GetCurrentMethod(), ex);
            //}
           
            bool isAuthenticated = false;

            var userID = string.Empty;

            var loginID = Guid.NewGuid().ToString();

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name.Substring(User.Identity.Name.IndexOf('\\') + 1);

                Logger.Log(string.Format("User \"{0}\" IsAuthenticated", User.Identity.Name));

#if ASE
                if (userID.StartsWith("C"))
                {
                    userID = userID.Substring(1);
                }
#endif

                Logger.Log(string.Format("UserID \"{0}\"", userID));

#if ASE
                try
                {
                    var loginDoc = XDocument.Load(LoginFile);

                    var login = loginDoc.Root.Elements("LOGIN").FirstOrDefault(x => x.Attribute("USERID").Value == userID);

                    if (login != null)
                    {
                        login.Attribute("LOGINID").Value = loginID;

                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            login.Attribute("RETURNURL").Value = ReturnUrl;
                        }
                        else
                        {
                            login.Attribute("RETURNURL").Value = "";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            loginDoc.Root.Add(new XElement("LOGIN", new XAttribute("USERID", userID), new XAttribute("LOGINID", loginID), new XAttribute("RETURNURL", ReturnUrl)));
                        }
                        else
                        {
                            loginDoc.Root.Add(new XElement("LOGIN", new XAttribute("USERID", userID), new XAttribute("LOGINID", loginID), new XAttribute("RETURNURL", "")));
                        }
                    }

                    loginDoc.Save(LoginFile);

                    isAuthenticated = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                }

#else
                try
                {
                    var loginDoc = XDocument.Load(LoginFile);

                    var login = loginDoc.Root.Elements("LOGIN").FirstOrDefault(x => x.Attribute("USERID").Value == userID);

                    if (login != null)
                    {
                        login.Attribute("LOGINID").Value = loginID;

                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            login.Attribute("RETURNURL").Value = ReturnUrl;
                        }
                        else
                        {
                            login.Attribute("RETURNURL").Value = "";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            loginDoc.Root.Add(new XElement("LOGIN", new XAttribute("USERID", userID), new XAttribute("LOGINID", loginID), new XAttribute("RETURNURL", ReturnUrl)));
                        }
                        else
                        {
                            loginDoc.Root.Add(new XElement("LOGIN", new XAttribute("USERID", userID), new XAttribute("LOGINID", loginID), new XAttribute("RETURNURL", "")));
                        }
                    }

                    loginDoc.Save(LoginFile);

                    isAuthenticated = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                }
#endif
            }
            else
            {
                try
                {
                    Logger.Log(string.Format("User \"{0}\" Not Authenticated", User.Identity.Name));
                }
                catch { }
            }

            var url = string.Empty;

            if (isAuthenticated)
            {
                url = string.Format("http://{0}/{1}?UserID={2}&LoginID={3}", Config.WebSite, Config.WindowsAuthentication, userID, loginID);
            }
            else
            {
                url = string.Format("http://{0}/{1}", Config.WebSite, Config.FormsAuthentication);
            }

            Logger.Log(string.Format("Url:{0}", url));

            return Redirect(url);
        }
    }
}