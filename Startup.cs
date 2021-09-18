using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MeuCantinhoDeEstudos3.Startup))]
namespace MeuCantinhoDeEstudos3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
