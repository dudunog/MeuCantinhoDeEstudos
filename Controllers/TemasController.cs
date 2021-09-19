using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MeuCantinhoDeEstudos3.Models;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using StackExchange.Profiling;
using System.Transactions;

namespace MeuCantinhoDeEstudos3.Controllers
{
    public class TemasController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: Temas
        public async Task<ActionResult> Index(string search)
        {
            ViewBag.CurrentSearch = search;

            var temas = db.Temas.Include(t => t.Materia);

            if (!string.IsNullOrEmpty(search))
            {
                temas = temas.Where(t => t.Nome.ToUpper().Contains(search.ToUpper()));
            }

            return View(await temas.ToListAsync());
        }

        // GET: Temas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tema tema = db.Temas
                          .Include(t => t.Materia)
                          .SingleOrDefault(t => t.TemaId == id);

            if (tema == null)
            {
                return HttpNotFound();
            }

            return View(tema);
        }

        // GET: Temas/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.MateriaId =
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId), "MateriaId", "Nome");

            return View();
        }

        // POST: Temas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TemaId,MateriaId,Nome")] Tema tema)
        {
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Entry(tema).State = EntityState.Added;
                    await db.SaveChangesAsync();

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            var userId = User.Identity.GetUserId<int>();

            ViewBag.MateriaId = 
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId), "MateriaId", "Nome", tema.MateriaId);
            
            return View(tema);
        }

        // GET: Temas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tema tema = await db.Temas.FindAsync(id);

            if (tema == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            ViewBag.MateriaId = 
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId), "MateriaId", "Nome", tema.MateriaId);

            return View(tema);
        }

        // POST: Temas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TemaId,MateriaId,Nome,DataCriacao,UsuarioCriacao")] Tema tema)
        {
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Entry(tema).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            var userId = User.Identity.GetUserId<int>();

            ViewBag.MateriaId =
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId), "MateriaId", "Nome", tema.MateriaId);

            return View(tema);
        }

        // GET: Temas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tema tema = await db.Temas.FindAsync(id);

            if (tema == null)
            {
                return HttpNotFound();
            }

            return View(tema);
        }

        // POST: Temas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Tema tema = await db.Temas.FindAsync(id);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                db.Entry(tema).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                scope.Complete();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
