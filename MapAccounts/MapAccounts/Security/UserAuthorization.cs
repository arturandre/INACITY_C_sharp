using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace MapAccounts.Security
{
    public class UserAuthorization : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            httpContext.Response.TrySkipIisCustomErrors = true;
            var authorized = base.AuthorizeCore(httpContext);
            if (!authorized)
            {
                return false;
            }

            var routeData = httpContext.Request.RequestContext.RouteData;
            var id = routeData.Values["id"];

            var userName = httpContext.User.Identity.Name;
            return base.AuthorizeCore(httpContext);
        }
    }
}