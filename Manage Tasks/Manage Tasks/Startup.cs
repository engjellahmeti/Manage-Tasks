using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Manage_Tasks.Startup))]
namespace Manage_Tasks
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
