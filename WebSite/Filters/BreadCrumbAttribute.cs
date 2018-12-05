using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Reflection;
using Utility;
using Models.Authenticated;

namespace WebSite.Filters
{
    public class BreadCrumbAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var account = HttpContext.Current.Session["Account"] as Account;

            try
            {
                filterContext.Controller.ViewBag.CurrentAncestorID = null;
                filterContext.Controller.ViewBag.CurrentAncestorPage = null;
                filterContext.Controller.ViewBag.CurrentParentID = null;
                filterContext.Controller.ViewBag.CurrentParentPage = null;
                filterContext.Controller.ViewBag.CurrentPageID = null;
                filterContext.Controller.ViewBag.CurrentPage = null;

                string area = string.Empty;

                if (filterContext.RouteData.DataTokens["area"] != null)
                {
                    area = filterContext.RouteData.DataTokens["area"].ToString();
                }

                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();

                if (controller != "Home")
                {
                    if (account != null)
                    {
                        if (!string.IsNullOrEmpty(area))
                        {
                            var ancestorPage = account.MenuItemList.FirstOrDefault(a => a.SubItemList.Any(x => x.Area == area && x.Controller == controller && x.Action == action));

                            if (ancestorPage != null)
                            {
                                var page = ancestorPage.SubItemList.FirstOrDefault(x => x.Area == area && x.Controller == controller && x.Action == action);

                                if (page != null)
                                {
                                    filterContext.Controller.ViewBag.CurrentAncestorID = ancestorPage.ID;
                                    filterContext.Controller.ViewBag.CurrentAncestorPage = ancestorPage.Description[filterContext.Controller.ViewBag.Lang];
                                    filterContext.Controller.ViewBag.CurrentPageID = page.ID;
                                    filterContext.Controller.ViewBag.CurrentPage = page.Description[filterContext.Controller.ViewBag.Lang];
                                }
                            }
                            else
                            {
                                foreach (var ancestor in account.MenuItemList)
                                {
                                    var parentPage = ancestor.SubItemList.FirstOrDefault(p => p.SubItemList.Any(x => x.Area == area && x.Controller == controller && x.Action == action));

                                    if (parentPage != null)
                                    {
                                        var page = parentPage.SubItemList.FirstOrDefault(x => x.Area == area && x.Controller == controller && x.Action == action);

                                        if (page != null)
                                        {
                                            filterContext.Controller.ViewBag.CurrentAncestorID = ancestor.ID;
                                            filterContext.Controller.ViewBag.CurrentAncestorPage = ancestor.Description[filterContext.Controller.ViewBag.Lang];
                                            filterContext.Controller.ViewBag.CurrentParentID = parentPage.ID;
                                            filterContext.Controller.ViewBag.CurrentParentPage = parentPage.Description[filterContext.Controller.ViewBag.Lang];
                                            filterContext.Controller.ViewBag.CurrentPageID = page.ID;
                                            filterContext.Controller.ViewBag.CurrentPage = page.Description[filterContext.Controller.ViewBag.Lang];
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var ancestorPage = account.MenuItemList.FirstOrDefault(a => a.SubItemList.Any(x => x.Controller == controller && x.Action == action));

                            if (ancestorPage != null)
                            {
                                var page = ancestorPage.SubItemList.FirstOrDefault(x => x.Controller == controller && x.Action == action);

                                if (page != null)
                                {
                                    filterContext.Controller.ViewBag.CurrentAncestorID = ancestorPage.ID;
                                    filterContext.Controller.ViewBag.CurrentAncestorPage = ancestorPage.Description[filterContext.Controller.ViewBag.Lang];
                                    filterContext.Controller.ViewBag.CurrentPageID = page.ID;
                                    filterContext.Controller.ViewBag.CurrentPage = page.Description[filterContext.Controller.ViewBag.Lang];
                                }
                            }
                            else
                            {
                                foreach (var ancestor in account.MenuItemList)
                                {
                                    var parentPage = ancestor.SubItemList.FirstOrDefault(p => p.SubItemList.Any(x => x.Controller == controller && x.Action == action));

                                    if (parentPage != null)
                                    {
                                        var page = parentPage.SubItemList.FirstOrDefault(x => x.Controller == controller && x.Action == action);

                                        if (page != null)
                                        {
                                            filterContext.Controller.ViewBag.CurrentAncestorID = ancestor.ID;
                                            filterContext.Controller.ViewBag.CurrentAncestorPage = ancestor.Description[filterContext.Controller.ViewBag.Lang];
                                            filterContext.Controller.ViewBag.CurrentParentID = parentPage.ID;
                                            filterContext.Controller.ViewBag.CurrentParentPage = parentPage.Description[filterContext.Controller.ViewBag.Lang];
                                            filterContext.Controller.ViewBag.CurrentPageID = page.ID;
                                            filterContext.Controller.ViewBag.CurrentPage = page.Description[filterContext.Controller.ViewBag.Lang];
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                filterContext.Controller.ViewBag.CurrentAncestorID = null;
                filterContext.Controller.ViewBag.CurrentAncestorPage = null;
                filterContext.Controller.ViewBag.CurrentParentID = null;
                filterContext.Controller.ViewBag.CurrentParentPage = null;
                filterContext.Controller.ViewBag.CurrentPageID = null;
                filterContext.Controller.ViewBag.CurrentPage = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}