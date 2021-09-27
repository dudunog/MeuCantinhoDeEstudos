using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using EntityFramework.Triggers;
using MeuCantinhoDeEstudos3.Models.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MeuCantinhoDeEstudos3.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class Usuario : IdentityUser<int, UsuarioLogin, UsuarioGrupo, UsuarioIdentificacao>, IEntidade
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Usuario, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public override int Id { get; set; }
        [Display(Name = "Endereço de E-mail")]
        public override string Email { get; set; }
        [Display(Name = "E-mail Confirmado?")]
        public override bool EmailConfirmed { get; set; }
        [Display(Name = "Hash de Senha")]
        public override string PasswordHash { get; set; }
        [Display(Name = "Carimbo de Segurança")]
        public override string SecurityStamp { get; set; }
        [Display(Name = "Número de Telefone")]
        public override string PhoneNumber { get; set; }
        [Display(Name = "Número de Telefone Confirmado")]
        public override bool PhoneNumberConfirmed { get; set; }
        [Display(Name = "Dois-Fatores Hablitado?")]
        public override bool TwoFactorEnabled { get; set; }
        [Display(Name = "Fim do Período de Conta Travada")]
        public override DateTime? LockoutEndDateUtc { get; set; }
        [Display(Name = "Travamento de Conta Habilitado?")]
        public override bool LockoutEnabled { get; set; }
        [Display(Name = "Contagem de Falhas de Acesso")]
        public override int AccessFailedCount { get; set; }
        [Display(Name = "Login")]
        public override string UserName { get; set; }
        public virtual UsuarioInformacoes UsuarioInformacoes { get; set; }
        public virtual ICollection<Materia> Materias { get; set; }
        public DateTime DataCriacao { get; set; }
        public string UsuarioCriacao { get; set; }
        public DateTime? UltimaModificacao { get; set; }
        public string UsuarioModificacao { get; set; }
    }
}