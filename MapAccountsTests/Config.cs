using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapAccountsTests
{
    public static class Config
    {

        public static String baseAzureUrl = "http://anurbs.azurewebsites.net/";
        public static String currentBaseUrl = baseAzureUrl;
        public static String createUrl(String apiBaseUrl, String callUrl)
        {
            return currentBaseUrl + apiBaseUrl + callUrl;
        }
    }
}
