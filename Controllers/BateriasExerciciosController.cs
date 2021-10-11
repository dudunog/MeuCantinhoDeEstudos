using ExcelDataReader;
using MeuCantinhoDeEstudos3.Extensions;
using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.ViewModels;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using RazorPDF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class BateriasExerciciosController : Extensions.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: BateriaExercicios
        public async Task<ActionResult> Index(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = search;

            var bateriasExercicios = db.BateriasExercicios
                                    .Include(b => b.Tema.Materia)
                                    .Where(b => b.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                bateriasExercicios = bateriasExercicios.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            return View(await bateriasExercicios.ToListAsync());
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
        public async Task<ActionResult> Create([Bind(Include = "TemaId,Descricao,QuantidadeExercicios,QuantidadeAcertos")] BateriaExercicioViewModel bateriaExercicio)
        {
            var userId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                var bateriaExercicioNovo = bateriaExercicio.ToBateriaExercicio();
                bateriaExercicioNovo.CalcularAproveitamento();

                db.BateriasExercicios.Add(bateriaExercicioNovo);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);
            var temas = db.Temas;

            ViewBag.MateriaId = new SelectList(materias, "MateriaId", "Nome");
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
        [HttpGet]
        public async Task<JsonResult> SelecionarPorMateria(int id)
        {
            var temas = db.Temas
                        .Include(t => t.Materia)
                        .Where(t => t.Materia.MateriaId == id);

            return Json(await temas.ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ImportarExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            var postedFile = Request.Files[0];
            List<BateriaExercicio> exercicios = new List<BateriaExercicio>();

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
                if (!(excelReader.IsDBNull(0) || excelReader.IsDBNull(1) || excelReader.IsDBNull(2)))
                {
                    try
                    {
                        var bateriaExercicio = new BateriaExercicio()
                        {
                            TemaId = int.Parse(excelReader.GetValue(0).ToString()),
                            Descricao = excelReader.GetString(1),
                            QuantidadeExercicios = int.Parse(excelReader.GetValue(2).ToString()),
                            QuantidadeAcertos = int.Parse(excelReader.GetValue(3).ToString()),
                            DataCriacao = DateTime.Now,
                            UsuarioCriacao = User.Identity.GetUserName(),
                        };

                        bateriaExercicio.CalcularAproveitamento();
                        
                        exercicios.Add(bateriaExercicio);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception();
                    }
                }
            }

            excelReader.Close();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    db.BateriasExercicios.AddRange(exercicios);
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
                excelPackage.Workbook.Properties.Title = "Relatório-Exercícios";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Exercícios");

                //Títulos
                var i = 1;
                var titulos = new String[] { "Descrição", "Quantidade de exercícios", "Quantidade de acertos", "Aproveitamento", "Matéria", "Tema" };
                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var bateriasExercicios = db.BateriasExercicios
                                         .Include(a => a.Tema.Materia)
                                         .Where(a => a.Tema.Materia.UsuarioId == userId);

                if (!string.IsNullOrEmpty(search))
                {
                    bateriasExercicios = bateriasExercicios.Where(e => e.Descricao.ToUpper().Contains(search.ToUpper()));
                }

                //Valores
                var r = 2;
                foreach (var exercicio in bateriasExercicios)
                {
                    sheet.Cells[r, 1].Value = exercicio.Descricao;
                    sheet.Cells[r, 2].Value = exercicio.QuantidadeExercicios;
                    sheet.Cells[r, 3].Value = exercicio.QuantidadeAcertos;

                    sheet.Cells[r, 4].Style.Numberformat.Format = "0.00%";
                    sheet.Cells[r, 4].Value = exercicio.Aproveitamento/100;
                    
                    sheet.Cells[r, 5].Value = exercicio.Tema.Materia.Nome;
                    sheet.Cells[r++, 6].Value = exercicio.Tema.Nome;
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

            var bateriasExercicios = db.BateriasExercicios
                                     .Include(a => a.Tema.Materia)
                                     .Where(a => a.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                bateriasExercicios = bateriasExercicios.Where(e => e.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            return new PdfActionResult("BateriasExerciciosReport", bateriasExercicios.ToList())
            {
                FileDownloadName = "Relatório-Exercícios.pdf"
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