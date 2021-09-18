using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using MeuCantinhoDeEstudos3.Models;

namespace MeuCantinhoDeEstudos3
{
    // Configure the application sign-in manager which is used in this application.
    public class GerenciadorLogin : SignInManager<Usuario, int>
    {
        public GerenciadorLogin(GerenciadorUsuarios userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(Usuario user)
        {
            return user.GenerateUserIdentityAsync(UserManager);
        }

        public static GerenciadorLogin Create(IdentityFactoryOptions<GerenciadorLogin> options, IOwinContext context)
        {
            return new GerenciadorLogin(context.GetUserManager<GerenciadorUsuarios>(), context.Authentication);
        }
    }
}
