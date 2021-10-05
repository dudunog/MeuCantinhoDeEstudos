using ExcelDataReader;
using MeuCantinhoDeEstudos3.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class AtividadesController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: Atividades
        public async Task<ActionResult> Index(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = search;

            var atividades = db.Atividades
                           .Include(a => a.Tema.Materia)
                           .Where(m => m.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                atividades = atividades.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
            }
           
            return View(await atividades.ToListAsync());
        }

        // GET: Atividades/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Atividade atividade = await db.Atividades.FindAsync(id);
            if (atividade == null)
            {
                return HttpNotFound();
            }
            return View(atividade);
        }

        // GET: Atividades/Create
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

        // POST: Atividades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TemaId,Descricao")] Atividade atividade)
        {
            var userId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                db.Atividades.Add(atividade);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.MateriaId =
                new SelectList(db.Materias
                                .Where(m => m.UsuarioId == userId).ToList(),
                                "MateriaId",
                                "Nome", atividade.Materia.MateriaId);

            ViewBag.TemaId = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(atividade);
        }

        // GET: Atividades/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Atividade atividade = await db.Atividades.FindAsync(id);

            if (atividade == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            ViewBag.TemaId =
                new SelectList(db.Temas
                               .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == atividade.Tema.MateriaId)
                               .ToList(),
                               "TemaId",
                               "Nome",
                               atividade.TemaId);

            ViewBag.MateriaId =
                new SelectList(db.Materias
                               .Where(m => m.UsuarioId == userId)
                               .ToList(),
                               "MateriaId",
                               "Nome",
                               atividade.Tema.MateriaId);

            return View(atividade);
        }

        // POST: Atividades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,TemaId,Descricao")] Atividade atividade)
        {
            if (ModelState.IsValid)
            {
                db.Entry(atividade).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(atividade);
        }

        // GET: Atividades/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Atividade atividade = await db.Atividades.FindAsync(id);
            if (atividade == null)
            {
                return HttpNotFound();
            }
            return View(atividade);
        }

        // POST: Atividades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Atividade atividade = await db.Atividades.FindAsync(id);
            db.Atividades.Remove(atividade);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> ImportarExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            var postedFile = Request.Files[0];
            var atividades = new List<Atividade>();

            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(postedFile.InputStream);

            DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true,
                }
            });

            while (excelReader.Read())
            {
                if (!(excelReader.IsDBNull(0) || excelReader.IsDBNull(1)))
                {
                    atividades.Add(new Atividade
                    {
                        TemaId = int.Parse(excelReader.GetValue(0).ToString()),
                        Descricao = excelReader.GetString(1),
                        DataCriacao = DateTime.Now,
                        UsuarioCriacao = User.Identity.GetUserName(),
                    });
                }
            }

            excelReader.Close();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                //db.BulkInsert(atividades);
                db.Atividades.AddRange(atividades);
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