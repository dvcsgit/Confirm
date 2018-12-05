using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Utility.Models;
using Models.Authenticated;
using Models.EquipmentMaintenance.QFormManagement;
using Utility;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Linq;

#if ASE
using DbEntity.ASE;
using DataAccess.ASE;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using DataAccess.ASE.QA;
using System.DirectoryServices;
#else
using DbEntity.MSSQL;
using DataAccess;
using DataAccess.EquipmentMaintenance;
using System.Collections.Generic;
using System.Xml.Linq;
using System.DirectoryServices;
#endif


namespace WebSite.Controllers
{
    public class HomeController : Controller
    {
#if ASE
#if QAS
        private string LoginFile = @"C:\FEM\WindowsAuthentication.xml";
#else
        private string LoginFile = @"D:\FEM\WindowsAuthentication.xml";
#endif

#if DEBUG
        private string ConfigFile = @"D:\Project\[FEM]\FEM\Source Code\FEM\DataAccess.ASE\IE10.xml";
#else
#if QAS
        private string ConfigFile = @"C:\FEM\Portal\bin\IE10.xml";
#else
        private string ConfigFile = @"D:\FEM\Portal\bin\IE10.xml";
#endif
#endif

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Portal(string UserID, string LoginID)
        {
            Logger.Log("Portal Login");
            Logger.Log(string.Format("UserID:{0}", UserID));
            Logger.Log(string.Format("LoginID:{0}", LoginID));
            Logger.Log(string.Format("IP Address:{0}", Request.UserHostName));

            //Windows驗證登入, 先檢查隨機碼LoginID是否正確, 避免直接使用UserID登入

            var loginDoc = XDocument.Load(LoginFile);

            var login = loginDoc.Root.Elements("LOGIN").FirstOrDefault(x => x.Attribute("USERID").Value == UserID && x.Attribute("LOGINID").Value == LoginID);

            if (login != null)
            {
                Logger.Log("Account IsAuthenticated");

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = AccountDataAccessor.GetAccount(organizationList, UserID);

                if (result.IsSuccess)
                {
                    var account = result.Data as Account;

                    var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                    string encTicket = FormsAuthentication.Encrypt(ticket);
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                    Session["Account"] = account;

                    Session["ReturnUrl"] = login.Attribute("RETURNURL").Value;

                    Logger.Log("Login Success");

                    return RedirectToAction("Index");
                }
                else
                {
                    Logger.Log("Login Failed");

                    return RedirectToAction("Login");
                }
            }
            else
            {
                Logger.Log("Account Not Authenticated");

                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            var root = XDocument.Load(ConfigFile).Root;

            var elementList = root.Elements("RECORD").ToList();

            var machineName = Request.UserHostName;

            if (elementList.Any(x => x.Value == machineName))
            {
                return View(new LoginFormModel());
            }
            else
            {
                return RedirectToAction("Compatibility");
            }
        }

#if DEBUG
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginFormModel Model)
        {
            Logger.Log("Form Login");
            Logger.Log(string.Format("UserID:{0}", Model.UserID));
            Logger.Log(string.Format("Password:{0}", Model.Password));
            Logger.Log(string.Format("IP Address:{0}", Request.UserHostName));

            RequestResult result = AccountDataAccessor.GetAccount(Model);

            if (result.IsSuccess)
            {
                var user = result.Data as ACCOUNT;

                if (string.Compare(Define.UtilityPassword, Model.Password, false) == 0 || string.Compare(user.PASSWORD, Model.Password, false) == 0)
                {
                    var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                    result = AccountDataAccessor.GetAccount(organizationList, user);

                    if (result.IsSuccess)
                    {
                        var account = result.Data as Account;

                        var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                        string encTicket = FormsAuthentication.Encrypt(ticket);
                        Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                        Session["Account"] = account;

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("UserID", result.Message);

                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("UserID", result.Message);

                return View();
            }
        }
#endif

#if !DEBUG
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginFormModel Model)
        {
            Logger.Log("Form Login");
            Logger.Log(string.Format("UserID:{0}", Model.UserID));
            Logger.Log(string.Format("Password:{0}", Model.Password));
            Logger.Log(string.Format("IP Address:{0}", Request.UserHostName));

            RequestResult result = AccountDataAccessor.GetAccount(Model);

            if (result.IsSuccess)
            {
                var user = result.Data as ACCOUNT;

                if (Config.HaveLDAPSetting && user.ID != "admin" && string.Compare(Model.Password, Define.UtilityPassword, false)!=0)
                {
                    try
                    {
                        using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + Config.LDAP_Domain, Config.LDAP_UserID, Config.LDAP_Password))
                        {
                            using (DirectorySearcher searcher = new DirectorySearcher(entry))
                            {
                                searcher.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + "C" + Model.UserID + "))";

                                var userObject = searcher.FindOne();

                                if (userObject != null)
                                {
                                    using (DirectoryEntry tryLogin = new DirectoryEntry("LDAP://" + Config.LDAP_Domain, "C" + Model.UserID, Model.Password))
                                    {
                                        using (DirectorySearcher trySearch = new DirectorySearcher(tryLogin))
                                        {
                                            trySearch.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + "C" + Model.UserID + "))";

                                            try
                                            {
                                                var test = trySearch.FindOne();

                                                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                                                result = AccountDataAccessor.GetAccount(organizationList, user);

                                                if (result.IsSuccess)
                                                {
                                                    var account = result.Data as Account;

                                                    var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                                                    string encTicket = FormsAuthentication.Encrypt(ticket);
                                                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                                                    Session["Account"] = account;

                                                    return RedirectToAction("Index");
                                                }
                                                else
                                                {
                                                    ModelState.AddModelError("UserID", result.Message);

                                                    return View();
                                                }
                                            }
                                            catch
                                            {
                                                ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                                                return View();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (string.Compare(user.PASSWORD, Model.Password, false) == 0)
                                    {
                                        var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                                        result = AccountDataAccessor.GetAccount(organizationList, user);

                                        if (result.IsSuccess)
                                        {
                                            var account = result.Data as Account;

                                            var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                                            string encTicket = FormsAuthentication.Encrypt(ticket);
                                            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                                            Session["Account"] = account;

                                            return RedirectToAction("Index");
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("UserID", result.Message);

                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                                        return View();
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("UserID", "登入時發生錯誤");

                        return View();
                    }
                }
                else
                {
                    if (string.Compare(user.PASSWORD, Model.Password, false) == 0 || string.Compare(Define.UtilityPassword, Model.Password, false) == 0)
                    {
                        var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                        result = AccountDataAccessor.GetAccount(organizationList, user);

                        if (result.IsSuccess)
                        {
                            var account = result.Data as Account;

                            var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                            string encTicket = FormsAuthentication.Encrypt(ticket);
                            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                            Session["Account"] = account;

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("UserID", result.Message);

                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                        return View();
                    }
                }
            }
            else
            {
                ModelState.AddModelError("UserID", result.Message);

                return View();
            }
        }
#endif

        public ActionResult Index()
        {
            if (Session["ReturnUrl"] != null)
            {
                ViewBag.ReturnUrl = Session["ReturnUrl"].ToString();

                Session.Remove("ReturnUrl");
            }

            return View("Index_ASE");
        }

        [AllowAnonymous]
        public ActionResult IE10()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult IE10Download(string Lang)
        {
            var root = XDocument.Load(ConfigFile).Root;

            if (Lang == "en-us")
            {
                return File(root.Element("IE10").Attribute("en_us").Value, "application/exe", "IE10_en_us.exe");
            }
            else
            {
                return File(root.Element("IE10").Attribute("zh_tw").Value, "application/exe", "IE10_zh_tw.exe");
            }
        }

        [AllowAnonymous]
        public ActionResult SetCompatibility()
        {
            var doc = XDocument.Load(ConfigFile);

            var machineName = Request.UserHostName;

            doc.Root.Add(new XElement("RECORD", machineName));

            doc.Save(ConfigFile);

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Compatibility()
        {
            return View("Compatibility");
        }

        [AllowAnonymous]
        public ActionResult CompatibilityDownload()
        {
            var doc = XDocument.Load(ConfigFile);

            var machineName = Request.UserHostName;

            doc.Root.Add(new XElement("RECORD", machineName));

            doc.Save(ConfigFile);

            return File(doc.Root.Element("Compatibility").Value, "application/exe", "IE10_Compatibility.exe");
        }

        public ActionResult GetAbnormalNotifyList()
        {
            RequestResult result = AbnormalNotifyDataAccessor.Query(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_AbnormalNotifyList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetCalibrationFormList()
        {
            RequestResult result = CalibrationFormDataAccessor.Query(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_CalibrationFormList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        } 
#else

#if PSI

        private string LoginFile = @"C:\FEM\WindowsAuthentication.xml";
#else
        private string LoginFile = @"D:\FEM\WindowsAuthentication.xml";
#endif

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginFormModel());
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginFormModel Model)
        {
            RequestResult result = AccountDataAccessor.GetAccount(Model);

            if (result.IsSuccess)
            {
                var user = result.Data as User;

                if (Config.HaveLDAPSetting)
                {
                    if (user.ID != "admin")
                    {
                        using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + Config.LDAP_Domain, Config.LDAP_UserID, Config.LDAP_Password))
                        {
                            using (DirectorySearcher searcher = new DirectorySearcher(entry))
                            {
                                searcher.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + Model.UserID + "))";

                                var userObject = searcher.FindOne();

                                if (userObject != null)
                                {
                                    using (DirectoryEntry tryLogin = new DirectoryEntry("LDAP://" + Config.LDAP_Domain, Model.UserID, Model.Password))
                                    {
                                        using (DirectorySearcher trySearch = new DirectorySearcher(tryLogin))
                                        {
                                            trySearch.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + Model.UserID + "))";

                                            try
                                            {
                                                var test = trySearch.FindOne();

                                                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                                                result = AccountDataAccessor.GetAccount(organizationList, user);

                                                if (result.IsSuccess)
                                                {
                                                    var account = result.Data as Account;

                                                    var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                                                    string encTicket = FormsAuthentication.Encrypt(ticket);
                                                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                                                    Session["Account"] = account;

                                                    return RedirectToAction("Index");
                                                }
                                                else
                                                {
                                                    ModelState.AddModelError("UserID", result.Message);

                                                    return View();
                                                }
                                            }
                                            catch
                                            {
                                                ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                                                return View();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (string.Compare(user.Password, Model.Password, false) == 0)
                                    {
                                        var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                                        result = AccountDataAccessor.GetAccount(organizationList, user);

                                        if (result.IsSuccess)
                                        {
                                            var account = result.Data as Account;

                                            var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                                            string encTicket = FormsAuthentication.Encrypt(ticket);
                                            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                                            Session["Account"] = account;

                                            return RedirectToAction("Index");
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("UserID", result.Message);

                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                                        return View();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (string.Compare(user.Password, Model.Password, false) == 0)
                        {
                            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                            result = AccountDataAccessor.GetAccount(organizationList, user);

                            if (result.IsSuccess)
                            {
                                var account = result.Data as Account;

                                var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                                string encTicket = FormsAuthentication.Encrypt(ticket);
                                Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                                Session["Account"] = account;

                                return RedirectToAction("Index");
                            }
                            else
                            {
                                ModelState.AddModelError("UserID", result.Message);

                                return View();
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                            return View();
                        }
                    }
                }
                else
                {
                    if (string.Compare(user.Password, Model.Password, false) == 0)
                    {
                        var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                        result = AccountDataAccessor.GetAccount(organizationList, user);

                        if (result.IsSuccess)
                        {
                            var account = result.Data as Account;

                            var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                            string encTicket = FormsAuthentication.Encrypt(ticket);
                            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                            Session["Account"] = account;

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("UserID", result.Message);

                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", Resources.Resource.WrongPassword);

                        return View();
                    }
                }
            }
            else
            {
                ModelState.AddModelError("UserID", result.Message);

                return View();
            }
        }

        public ActionResult Index()
        {
            if (Session["ReturnUrl"] != null)
            {
                ViewBag.ReturnUrl = Session["ReturnUrl"].ToString();

                Session.Remove("ReturnUrl");
            }

            return View();
        }

         [AllowAnonymous]
        [HttpGet]
        public ActionResult Portal(string UserID, string LoginID)
        {
            Logger.Log("Portal Login");
            Logger.Log(string.Format("UserID:{0}", UserID));
            Logger.Log(string.Format("LoginID:{0}", LoginID));
            Logger.Log(string.Format("IP Address:{0}", Request.UserHostName));

            //Windows驗證登入, 先檢查隨機碼LoginID是否正確, 避免直接使用UserID登入

            var loginDoc = XDocument.Load(LoginFile);

            var login = loginDoc.Root.Elements("LOGIN").FirstOrDefault(x => x.Attribute("USERID").Value == UserID && x.Attribute("LOGINID").Value == LoginID);

            if (login != null)
            {
                Logger.Log("Account IsAuthenticated");

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = AccountDataAccessor.GetAccount(organizationList, UserID);

                if (result.IsSuccess)
                {
                    var account = result.Data as Account;

                    var ticket = new FormsAuthenticationTicket(1, account.ID, DateTime.Now, DateTime.Now.AddHours(24), true, account.ID, FormsAuthentication.FormsCookiePath);
                    string encTicket = FormsAuthentication.Encrypt(ticket);
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                    Session["Account"] = account;

                    Session["ReturnUrl"] = login.Attribute("RETURNURL").Value;

                    Logger.Log("Login Success");

                    return RedirectToAction("Index");
                }
                else
                {
                    Logger.Log("Login Failed");

                    return RedirectToAction("Login");
                }
            }
            else
            {
                Logger.Log("Account Not Authenticated");

                return RedirectToAction("Login");
            }
        }
#endif

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View(new PasswordFormModel());
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(AccountDataAccessor.ChangePassword(Model, Session["Account"] as Account)));
        }

        [HttpGet]
        public ActionResult ChangeUserPhoto()
        {
            return View(new UserPhotoFormModel());
        }

        [HttpPost]
        public ActionResult ChangeUserPhoto(UserPhotoFormModel Model)
        {
            RequestResult result = AccountDataAccessor.ChangeUserPhoto(Model, Session["Account"] as Account);

            return View("ChangeUserPhoto", new UserPhotoFormModel()
            {
                RequestResult = result
            });
        }

        [AllowAnonymous]
        public ActionResult Signout()
        {
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Error()
        {
            return View("Error");
        }

        public ActionResult GetVerifyList()
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = HomeIndexHelper.GetVerifyList(accountList, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_VerifyList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetAbnormalHandlingList()
        {
#if CHIMEI
            RequestResult result = Customized.CHIMEI.DataAccess.HomeIndexHelper.GetAbnormalHandlingList(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_AbnormalHandlingList_CHIMEI", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
#else
            RequestResult result = HomeIndexHelper.GetAbnormalHandlingList(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_AbnormalHandlingList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
#endif
        }

        public ActionResult GetRepairFormList()
        {
            RequestResult result = HomeIndexHelper.GetRepairFormList(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_RepairFormList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetEquipmentPatrolList()
        {
            RequestResult result = JobResultDataAccessor.Query(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_EquipmentPatrolList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetMaintenanceFormList()
        {
            RequestResult result = HomeIndexHelper.GetMaintenanceFormList(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_MaintenanceFormList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [AllowAnonymous]
        public ActionResult Flot()
        {
            return View();
        }

       

#if CIIC
        [AllowAnonymous]
        public ActionResult QForm(string UserID)
        {
            if (string.IsNullOrEmpty(UserID))
            {
                ViewBag.Error = new Error(MethodBase.GetCurrentMethod(), Resources.Resource.Unauthorized);

                return View("Error");
            }
            else
            {
                return View(new Models.EquipmentMaintenance.QFormManagement.CreateFormModel());
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateQForm(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Session["Captcha"] == null || string.IsNullOrEmpty(Session["Captcha"].ToString()))
                {
                    result.ReturnFailedMessage(Resources.Resource.RefreshCaptcha);
                }
                else if (Model.FormInput.Captcha.ToUpper() != Session["Captcha"].ToString().ToUpper())
                {
                    result.ReturnFailedMessage(Resources.Resource.CaptchaError);
                }
                else
                {
                    result = QFormDataAccessor.Create(Model);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult GetQFormList()
        {
            RequestResult result = QFormDataAccessor.Query();

            if (result.IsSuccess)
            {
                return PartialView("_QFormList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }
#endif
    }
}