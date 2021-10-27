using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MeuCantinhoDeEstudos3.Models;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Transactions;
using System;
using ExcelDataReader;
using System.Data;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Web;
using RazorPDF;
using MeuCantinhoDeEstudos3.ViewModels;
using AutoMapper;
using MeuCantinhoDeEstudos3.Mappers;
using MeuCantinhoDeEstudos3.Extensions;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class MateriasController : System.Web.Mvc.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: Materias
        public async Task<ActionResult> Index(string ordemClassificacao, string filtroAtual, string search, int? numeroPagina)
        {
            var userId = User.Identity.GetUserId<int>();

            if (search != null)
            {
                numeroPagina = 1;
            }
            else
            {
                search = filtroAtual;
            }

            var request = new FiltroRequest
            {
                OrdemClassificacao = ordemClassificacao,
                Search = search,
                NumeroPagina = numeroPagina
            };

            ViewBag.FiltroAtual = search;
            ViewBag.CurrentSearch = !string.IsNullOrEmpty(search) ? search : filtroAtual;
            ViewBag.ClassificacaoAtual = ordemClassificacao;
            ViewBag.ParametroClassificacaoNome = string.IsNullOrEmpty(ordemClassificacao) ? "name_desc" : "";
            ViewBag.ParametroClassificacaoData = ordemClassificacao == "Date" ? "date_desc" : "Date";

            var paginatedList = await BuscarMaterias(userId, request);

            return View(paginatedList);
        }

        private async Task<PaginatedList<Materia>> BuscarMaterias(int userId, FiltroRequest request)
        {
            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            switch (request.OrdemClassificacao)
            {
                case "Date":
                    materias = materias.OrderBy(m => m.DataCriacao);
                    break;
                case "date_desc":
                    materias = materias.OrderByDescending(m => m.DataCriacao);
                    break;
                case "name_desc":
                    materias = materias.OrderByDescending(m => m.Nome);
                    break;
                default:
                    materias = materias.OrderBy(s => s.Nome);
                    break;
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                materias = materias.Where(m => m.Nome.ToUpper().Contains(request.Search.ToUpper()));
            }

            int tamanhoPagina = 100;

            return await PaginatedList<Materia>.CreateAsync(materias, request.NumeroPagina ?? 1, tamanhoPagina);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "UsuarioId,Nome,CorIdentificacao")] Materia materia)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(materia.CorIdentificacao))
                    materia.CorIdentificacao = RemovePrefix(materia.CorIdentificacao);

                materia.UsuarioId = User.Identity.GetUserId<int>();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Entry(materia).State = EntityState.Added;
                    await db.SaveChangesAsync();

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MateriaId,Nome,CorIdentificacao,DataCriacao,UsuarioCriacao")] Materia materia)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(materia.CorIdentificacao))
                    materia.CorIdentificacao = RemovePrefix(materia.CorIdentificacao);
                
                materia.UsuarioId = User.Identity.GetUserId<int>();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Entry(materia).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            return View(materia);
        }

        // GET: Materias/Delete/5
        public ActionResult Delete(int? id)
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

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                db.Entry(materia).State = EntityState.Deleted;
                await db.SaveChangesAsync();

                scope.Complete();
            }

            return RedirectToAction("Index");
        }

        private static string RemovePrefix(string corIdentificacao)
        {
            return corIdentificacao.Replace("#", "");
        }

        [HttpPost]
        public async Task<ActionResult> InserirPorExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            
            if (Request.Files.Count > 0)
            {
                var postedFile = Request.Files[0];

                List<Materia> materias = new List<Materia>();

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
                        materias.Add(new Materia
                        {
                            Nome = excelReader.GetString(0),
                            CorIdentificacao = excelReader.GetString(1),
                            UsuarioId = userId,
                            DataCriacao = DateTime.Now,
                            UsuarioCriacao = User.Identity.GetUserName(),
                        });
                    }
                }

                excelReader.Close();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Materias.AddRange(materias);
                    await db.MyBulkInsertAsync(materias);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            return View("Create");
        }

        public ActionResult AtualizarPorExcel()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AtualizarPorExcelPost()
        {
            var userId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                var postedFile = Request.Files[0];

                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(postedFile.InputStream);

                DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true,
                    }
                });

                var materias = new List<Materia>();

                while (excelReader.Read())
                {
                    if (!(excelReader.IsDBNull(0) || excelReader.IsDBNull(1)))
                    {
                        try
                        {
                            var materiaId = excelReader.GetDouble(0);

                            var materia = await db.Materias
                                          .Include(m => m.Usuario)
                                          .FirstAsync(m => m.UsuarioId == userId && m.MateriaId == materiaId);
                            materia.Nome = excelReader.GetString(1);
                            materia.CorIdentificacao = excelReader.GetString(2);

                            db.Entry(materia).State = EntityState.Modified;
                            materias.Add(materia);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }

                excelReader.Close();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await db.MyBulkUpdateAsync(materias);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public FileResult GerarRelatorioExcel(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Meu Cantinho de Estudos";
                excelPackage.Workbook.Properties.Title = "Relatório-Matérias";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Matérias");

                //Títulos
                var i = 1;
                var titulos = new String[] { "Matéria", "Cor de Identificação" };
                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var materias = db.Materias
                               .Include(m => m.Usuario)
                               .Where(m => m.UsuarioId == userId);

                if (!string.IsNullOrEmpty(search))
                {
                    materias = materias.Where(m => m.Nome.ToUpper().Contains(search.ToUpper()));
                }

                //Valores
                var r = 2;
                foreach (var materia in materias)
                {
                    sheet.Cells[r, 1].Value = materia.Nome;
                    sheet.Cells[r++, 2].Value = materia.CorIdentificacao;
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

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                materias = materias.Where(m => m.Nome.ToUpper().Contains(search.ToUpper()));
            }

            return new PdfActionResult("MateriasReport", materias.ToList())
            {
                FileDownloadName = "Relatório-Matérias.pdf"
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
