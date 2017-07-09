using MapAccounts.Resources;
using System.Web;
using System.Web.Mvc;

namespace MapAccounts
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LocalizationAttribute("en"), 0);
        }
    }
}
