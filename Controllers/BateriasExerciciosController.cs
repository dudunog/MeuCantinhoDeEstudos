using MeuCantinhoDeEstudos3.Extensions;
using MeuCantinhoDeEstudos3.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class BateriasExerciciosController : Extensions.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: BateriaExercicios
        public async Task<ActionResult> Index()
        {
            return View(await db.BateriasExercicios
                              .Include(bt => bt.Tema)
                              .Include(t => t.Materia)
                              .ToListAsync());
        }

        // GET: BateriaExercicios/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BateriaExercicio bateriaExercicio = await db.BateriasExercicios.FindAsync(id);

            if (bateriaExercicio == null)
            {
                return HttpNotFound();
            }

            return View(bateriaExercicio);
        }

        // GET: BateriaExercicios/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.MateriaId = 
                new SelectList(db.Materias
                                .Where(m => m.UsuarioId == userId).ToList(), 
                                "MateriaId",
                                "Nome");

            ViewBag.TemaId = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }

        // POST: BateriaExercicios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TemaId,Descricao,QuantidadeExercicios,QuantidadeAcertos")] BateriaExercicio bateriaExercicio)
        {
            var userId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                bateriaExercicio.CalcularAproveitamento();

                db.BateriasExercicios.Add(bateriaExercicio);
                await db.SaveChangesAsync();

                //ViewBag.TemaId = new SelectList(Enumerable.Empty<SelectListItem>());
                return RedirectToAction("Index");
            }

            ViewBag.MateriaId =
                new SelectList(db.Materias
                                .Where(m => m.UsuarioId == userId).ToList(),
                                "MateriaId",
                                "Nome", bateriaExercicio.Materia.MateriaId);

            ViewBag.TemaId = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(bateriaExercicio);
        }

        // GET: BateriaExercicios/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BateriaExercicio bateriaExercicio = await db.BateriasExercicios.FindAsync(id);

            if (bateriaExercicio == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            ViewBag.TemaId =
                new SelectList(db.Temas
                               .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == bateriaExercicio.Tema.MateriaId)
                               .ToList(), 
                               "TemaId", 
                               "Nome", 
                               bateriaExercicio.TemaId);
            
            ViewBag.MateriaId =
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId)
                               .ToList(), 
                               "MateriaId", 
                               "Nome", 
                               bateriaExercicio.Tema.MateriaId);

            return View(bateriaExercicio);
        }

        // POST: BateriaExercicios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,Descricao,MateriaId,TemaId,QuantidadeExercicios,QuantidadeAcertos")] BateriaExercicio bateriaExercicio)
        {
            if (ModelState.IsValid)
            {
                bateriaExercicio.CalcularAproveitamento();
                db.Entry(bateriaExercicio).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(bateriaExercicio);
        }

        // GET: BateriaExercicios/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BateriaExercicio bateriaExercicio = await db.BateriasExercicios.FindAsync(id);

            if (bateriaExercicio == null)
            {
                return HttpNotFound();
            }

            return View(bateriaExercicio);
        }

        // POST: BateriaExercicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BateriaExercicio bateriaExercicio = await db.BateriasExercicios.FindAsync(id);
            db.BateriasExercicios.Remove(bateriaExercicio);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: BateriaExercicios/SelecionarPorMateria/5
        public async Task<JsonResult> SelecionarPorMateria(int id)
        {
            var temas = db.Temas.Where(t => t.MateriaId == id);

            return Json(await temas.ToListAsync(), JsonRequestBehavior.AllowGet);
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