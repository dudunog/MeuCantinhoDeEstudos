using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MeuCantinhoDeEstudos3.Models;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Transactions;
using ExcelDataReader;
using System.Data;
using System;
using OfficeOpenXml;
using System.Web;
using RazorPDF;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class TemasController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: Temas
        public async Task<ActionResult> Index(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = search;

            var temas = db.Temas
                        .Include(t => t.Materia)
                        .Where(t => t.Materia.UsuarioId == userId);

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

        [HttpPost]
        public async Task<ActionResult> ImportarExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            var postedFile = Request.Files[0];
            List<Tema> temas = new List<Tema>();

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
                try
                {
                    if (!(excelReader.IsDBNull(0) || excelReader.IsDBNull(1)))
                    {
                        temas.Add(new Tema
                        {
                            MateriaId = int.Parse(excelReader.GetValue(0).ToString()),
                            Nome = excelReader.GetString(1),
                            DataCriacao = DateTime.Now,
                            UsuarioCriacao = User.Identity.GetUserName(),
                        });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw new Exception();
                }
            }

            excelReader.Close();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    db.Temas.AddRange(temas);
                    await db.BulkSaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw new Exception();
                }

                scope.Complete();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public FileResult GerarRelatorioExcel(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Meu Cantinho de Estudos";
                excelPackage.Workbook.Properties.Title = "Relatório-Temas";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Temas");

                //Títulos
                var i = 1;
                var titulos = new String[] { "Tema", "Matéria" };
                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var temas = db.Temas
                            .Include(t => t.Materia)
                            .Where(t => t.Materia.UsuarioId == userId);

                if (!string.IsNullOrEmpty(search))
                {
                    temas = temas.Where(t => t.Nome.ToUpper().Contains(search.ToUpper()));
                }

                //Valores
                var r = 2;
                foreach (var tema in temas)
                {
                    sheet.Cells[r, 1].Value = tema.Nome;
                    sheet.Cells[r++, 2].Value = tema.Materia.Nome;
                }

                string fileName = $"{excelPackage.Workbook.Properties.Title}.xlsx";
                var contentType = MimeMapping.GetMimeMapping(fileName);
                var fileData = excelPackage.GetAsByteArray();

                return File(fileData, contentType, fileName);
            }
        }

        [HttpGet]
        public ActionResult GerarRelatorioPDF(string search)
        {
            //RazorPDF2
            var userId = User.Identity.GetUserId<int>();

            var temas = db.Temas
                            .Include(t => t.Materia)
                            .Where(t => t.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                temas = temas.Where(t => t.Nome.ToUpper().Contains(search.ToUpper()));
            }

            return new PdfActionResult("TemasReport", temas.ToList())
            {
                FileDownloadName = "Relatório-Temas.pdf"
            };
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
