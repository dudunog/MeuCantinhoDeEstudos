﻿using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MeuCantinhoDeEstudos3.Models;
using StackExchange.Profiling;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Transactions;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class MateriasController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: Materias
        public async Task<ActionResult> Index(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = search;

            var materias = db.Materias
                           .Where(m => m.UsuarioId == userId)
                           .Include(m => m.Usuario);

            if (!string.IsNullOrEmpty(search))
            {
                materias = materias.Where(m => m.Nome.ToUpper().Contains(search.ToUpper()));
            }

            return View(await materias.ToListAsync());
        }

        // GET: Materias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Materia materia = db.Materias
                                .Include(m => m.Temas)
                                .SingleOrDefault(m => m.MateriaId == id);

            if (materia == null)
            {
                return HttpNotFound();
            }

            return View(materia);
        }

        // GET: Materias/Create
        public ActionResult Create()
        {
            //ViewBag.UsuarioId = User.Identity.GetUserId();
            ViewData["UsuarioId"] = User.Identity.GetUserId();

            return View();
        }

        // POST: Materias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UsuarioId,Nome,CorIdentificacao")] Materia materia)
        {
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope())
                {
                    db.Entry(materia).State = EntityState.Added;
                    db.SaveChanges();

                    scope.Complete();
                }
                
                return RedirectToAction("Index");
            }

            ViewBag.UsuarioId = User.Identity.GetUserId<int>();

            return View(materia);
        }

        // GET: Materias/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Materia materia = await db.Materias.FindAsync(id);

            if (materia == null)
            {
                return HttpNotFound();
            }
            
            return View(materia);
        }

        // POST: Materias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MateriaId,Nome,CorIdentificacao")] Materia materia)
        {
            if (ModelState.IsValid)
            {
                materia.UsuarioId = User.Identity.GetUserId<int>();

                using (var scope = new TransactionScope())
                {
                    db.Entry(materia).State = EntityState.Modified;

                    //var camposModificados = db.ChangeTracker.Entries()
                    //    .Where(e => e.State == EntityState.Modified)
                    //    .Select(e => new {
                    //        EntidadeModificada = e.GetType().Name,
                    //        ValorAntigo = e.OriginalValues.PropertyNames.ToDictionary(pn => pn, pn => e.OriginalValues[pn]),
                    //        ValorNovo = e.CurrentValues.PropertyNames.ToDictionary(pn => pn, pn => e.CurrentValues[pn]),
                    //    });

                    db.SaveChanges();

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }
            
            return View(materia);
        }

        // GET: Materias/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Materia materia = db.Materias
                                 .Include(m => m.Temas)
                                 .SingleOrDefault(m => m.MateriaId == id);

            if (materia == null)
            {
                return HttpNotFound();
            }

            return View(materia);
        }

        // POST: Materias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Materia materia = await db.Materias.FindAsync(id);

            using (var scope = new TransactionScope())
            {
                db.Entry(materia).State = EntityState.Deleted;
                db.SaveChanges();

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
