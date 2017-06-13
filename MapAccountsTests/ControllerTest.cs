using System;

namespace MapAccountsTests
{
    public abstract class ControllerTest
    {
        public String apiBaseUrl;
        public ControllerTest(string _apiBaseUrl)
        {
            apiBaseUrl = _apiBaseUrl;
        }

        public String usualCallTest(String callUrl, String bodyParams)
        {
            HttpHelper httpHelper = new HttpHelper();
            return httpHelper.doPostString(Config.createUrl(apiBaseUrl, callUrl), bodyParams);
        }
    }
}