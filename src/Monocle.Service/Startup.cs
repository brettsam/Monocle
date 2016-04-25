using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Monocle.Service.Startup))]

namespace Monocle.Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}