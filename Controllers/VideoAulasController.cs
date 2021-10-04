using MeuCantinhoDeEstudos3.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class VideoAulasController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: VideoAulas
        public async Task<ActionResult> Index(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = search;

            var videoAulas = db.VideoAulas
                           .Include(v => v.Tema.Materia)
                           .Where(v => v.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                videoAulas = videoAulas.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            return View(await videoAulas.ToListAsync());
        }

        // GET: VideoAulas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VideoAula videoAula = await db.VideoAulas.FindAsync(id);
            if (videoAula == null)
            {
                return HttpNotFound();
            }
            return View(videoAula);
        }

        // GET: VideoAulas/Create
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

        // POST: VideoAulas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TemaId,Descricao,LinkVideo")] VideoAula videoAula)
        {
            var userId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                db.VideoAulas.Add(videoAula);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.MateriaId =
                new SelectList(db.Materias
                                .Where(m => m.UsuarioId == userId).ToList(),
                                "MateriaId",
                                "Nome", videoAula.Materia.MateriaId);

            ViewBag.TemaId = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(videoAula);
        }

        // GET: VideoAulas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            VideoAula videoAula = await db.VideoAulas.FindAsync(id);
            
            if (videoAula == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            ViewBag.TemaId =
                new SelectList(db.Temas
                               .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == videoAula.Tema.MateriaId)
                               .ToList(),
                               "TemaId",
                               "Nome",
                               videoAula.TemaId);

            ViewBag.MateriaId =
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId)
                               .ToList(),
                               "MateriaId",
                               "Nome",
                               videoAula.Tema.MateriaId);

            return View(videoAula);
        }

        // POST: VideoAulas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,TemaId,Descricao,LinkVideo")] VideoAula videoAula)
        {
            if (ModelState.IsValid)
            {
                db.Entry(videoAula).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(videoAula);
        }

        // GET: VideoAulas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VideoAula videoAula = await db.VideoAulas.FindAsync(id);
            if (videoAula == null)
            {
                return HttpNotFound();
            }
            return View(videoAula);
        }

        // POST: VideoAulas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            VideoAula videoAula = await db.VideoAulas.FindAsync(id);
            db.VideoAulas.Remove(videoAula);
            await db.SaveChangesAsync();
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