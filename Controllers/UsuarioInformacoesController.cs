using MeuCantinhoDeEstudos3.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class UsuarioInformacoesController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: UsuarioInformacoes/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId<int>();

            UsuarioInformacoes usuarioInformacoes = await db.UsuarioInformacoes.FindAsync(userId);

            if (usuarioInformacoes != null)
            {
                return RedirectToAction("Edit");
            }

            return View();
        }

        // POST: /UsuarioInformacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Pais,Estado,Cidade,CEP,Logradouro,Numero")] UsuarioInformacoes usuarioInformacoes)
        {
            if (ModelState.IsValid)
            {
                usuarioInformacoes.UsuarioId = User.Identity.GetUserId<int>();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Entry(usuarioInformacoes).State = EntityState.Added;
                    await db.SaveChangesAsync();

                    scope.Complete();
                }

                return RedirectToAction("Edit");
            }

            return RedirectToAction("Edit");
        }

        // GET: /UsuarioInformacoes/Edit
        public async Task<ActionResult> Edit()
        {
            var userId = User.Identity.GetUserId<int>();

            UsuarioInformacoes usuarioInformacoes = await db.UsuarioInformacoes.FindAsync(userId);

            if (usuarioInformacoes == null)
            {
                return HttpNotFound();
            }

            return View(usuarioInformacoes);
        }

        // POST: /UsuarioInformacoes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Pais,Estado,Cidade,CEP,Logradouro,Numero")] UsuarioInformacoes usuarioInformacoes)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId<int>();

                UsuarioInformacoes usuarioInformacoesOriginal =
                    await db.UsuarioInformacoes.FindAsync(userId);

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (TryUpdateModel(usuarioInformacoesOriginal))
                    {
                        db.Entry(usuarioInformacoesOriginal).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        
                        //db.SaveChanges();
                    }

                    scope.Complete();
                }

                return RedirectToAction("Edit");
            }

            return RedirectToAction("Edit");
        }
    }
}