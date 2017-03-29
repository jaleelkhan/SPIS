using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SPIS.Web.Startup))]

namespace SPIS.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}