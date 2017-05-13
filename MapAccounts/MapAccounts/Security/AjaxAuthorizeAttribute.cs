using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MapAccounts.Security
{
    public class AjaxAuthorizeAttribute : AuthorizeAttribute
    {
        
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            if (context.HttpContext.Request.IsAjaxRequest())
            {
                var urlHelper = new UrlHelper(context.RequestContext);
                context.HttpContext.Response.StatusCode = 403;
                var request = context.HttpContext.Request;
                var xpto = new System.IO.StreamReader(request.InputStream).ReadToEnd();
                var postbody = JsonConvert.DeserializeObject(xpto) ;
                context.Result = new JsonResult
                {
                    Data = new
                    {
                        Error = "NotAuthorized",
                        LogOnUrl = urlHelper.Action("Login", "Account", 
                        new { returnUrl = context.HttpContext.Request.RawUrl,
                        jo = postbody})
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                base.HandleUnauthorizedRequest(context);
            }
        }
    }
}