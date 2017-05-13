using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MapAccounts.Startup))]
namespace MapAccounts
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}