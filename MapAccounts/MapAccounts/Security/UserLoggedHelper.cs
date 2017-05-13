using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MapAccounts.Security
{
    public static class ControllerExtension
    {
        public static ActionResult IsUserLogged(this Controller c, Object Model)
        {
            var user = c.HttpContext.User;
            string ReturnAction = c.ControllerContext.RouteData.Values["action"].ToString();
            string ReturnController = c.ControllerContext.RouteData.Values["controller"].ToString();


            if (!user.Identity.IsAuthenticated)
            {
                c.HttpContext.Response.TrySkipIisCustomErrors = true;
                c.HttpContext.Response.StatusCode = 403;
                c.Session["Model"] = Model;
                var urlHelper = new UrlHelper(c.HttpContext.Request.RequestContext);
                return new JsonResult
                {
                    Data = new
                    {
                        Error = "NotAuthorized",
                        LogOnUrl = urlHelper.Action("Login", "Account",
                        new
                        {
                            returnUrl = "/" + ReturnController + "/" + ReturnAction
                        })
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return null;
            }
        }
    }
}