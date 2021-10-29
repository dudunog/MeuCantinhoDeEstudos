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
using MeuCantinhoDeEstudos3.ViewModels;
using AutoMapper;
using MeuCantinhoDeEstudos3.Mappers;
using MeuCantinhoDeEstudos3.Extensions;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class TemasController : System.Web.Mvc.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: Temas
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
            ViewBag.ParametroClassificacaoTema = string.IsNullOrEmpty(ordemClassificacao) ? "tema_desc" : "";
            ViewBag.ParametroClassificacaoMateria = ordemClassificacao == "materia" ? "materia_desc" : "materia";
            ViewBag.ParametroClassificacaoData = ordemClassificacao == "Date" ? "date_desc" : "Date";

            var paginatedList = await BuscarTemas(userId, request);

            return View(paginatedList);
        }

        private async Task<PaginatedList<Tema>> BuscarTemas(int userId, FiltroRequest request)
        {
            var temas = db.Temas
                        .Include(t => t.Materia.Usuario)
                        .Where(t => t.Materia.UsuarioId == userId);

            switch (request.OrdemClassificacao)
            {
                case "materia":
                    temas = temas.OrderBy(t => t.Materia.Nome);
                    break;
                case "materia_desc":
                    temas = temas.OrderByDescending(t => t.Materia.Nome);
                    break;
                case "Date":
                    temas = temas.OrderBy(t => t.DataCriacao);
                    break;
                case "date_desc":
                    temas = temas.OrderByDescending(t => t.DataCriacao);
                    break;
                case "tema_desc":
                    temas = temas.OrderByDescending(t => t.Nome);
                    break;
                default:
                    temas = temas.OrderBy(t => t.Nome);
                    break;
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                temas = temas.Where(t => t.Nome.ToUpper().Contains(request.Search.ToUpper()));
            }

            int tamanhoPagina = 100;

            return await PaginatedList<Tema>.CreateAsync(temas, request.NumeroPagina ?? 1, tamanhoPagina);
        }

        // GET: Temas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tema tema = await db.Temas
                        .Include(t => t.Materia)
                        .Include(t => t.Atividades)
                        .FirstOrDefaultAsync(t => t.TemaId == id);

            if (tema == null)
            {
                return HttpNotFound();
            }

            TemaViewModel viewModel =
                mapper.Map<TemaViewModel>(tema);

            return View(viewModel);
        }

        // GET: Temas/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId<int>();

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            TemaViewModel viewModel = new TemaViewModel
            {
                Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome")
            };

            return View(viewModel);
        }

        // POST: Temas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TemaId,MateriaId,Nome")] TemaViewModel viewModel)
        {
            Tema tema = mapper.Map<Tema>(viewModel);

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

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            viewModel.Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", viewModel.MateriaId);

            return View(viewModel);
        }

        // GET: Temas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Tema tema = await db.Temas
                              .Include(t => t.Materia)
                              .Include(t => t.Atividades)
                              .FirstOrDefaultAsync(t => t.TemaId == id);

            if (tema == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();
            
            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            TemaViewModel viewModel =
                mapper.Map<TemaViewModel>(tema);

            viewModel.Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", tema.MateriaId);

            return View(viewModel);
        }

        // POST: Temas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TemaId,MateriaId,Nome,DataCriacao,UsuarioCriacao")] TemaViewModel viewModel)
        {
            Tema tema = mapper.Map<Tema>(viewModel);

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

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            viewModel.Materias =
                new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", viewModel.MateriaId);

            return View(viewModel);
        }

        // GET: Temas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tema tema = await db.Temas
                              .Include(t => t.Materia)
                              .Include(t => t.Atividades)
                              .FirstOrDefaultAsync(t => t.TemaId == id);

            if (tema == null)
            {
                return HttpNotFound();
            }

            TemaViewModel viewModel = mapper.Map<TemaViewModel>(tema);

            return View(viewModel);
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
        public async Task<ActionResult> InserirPorExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            
            if (Request.Files.Count > 0)
            {
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
                    db.Temas.AddRange(temas);
                    await db.MyBulkInsertAsync(temas);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            TemaViewModel viewModel = new TemaViewModel
            {
                Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome")
            };

            return View("Create", viewModel);
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

                List<Tema> temas = new List<Tema>();

                while (excelReader.Read())
                {
                    if (!(excelReader.IsDBNull(0) || excelReader.IsDBNull(1)))
                    {
                        try
                        {
                            var temaId = excelReader.GetDouble(0);

                            var tema = await db.Temas
                                             .Include(t => t.Materia.Usuario)
                                             .FirstAsync(t => t.Materia.UsuarioId == userId && t.TemaId == temaId);
                            tema.Nome = excelReader.GetString(1);

                            db.Entry(tema).State = EntityState.Modified;
                            temas.Add(tema);
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
                    await db.MyBulkUpdateAsync(temas);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            return View();
        }

        private IQueryable<Tema> BuscarTemasExportacao(FiltroRequest request)
        {
            var userId = User.Identity.GetUserId<int>();

            var temas = db.Temas
                        .Include(t => t.Materia)
                        .Where(t => t.Materia.UsuarioId == userId);

            switch (request.OrdemClassificacao)
            {
                case "materia":
                    temas = temas.OrderBy(t => t.Materia.Nome);
                    break;
                case "materia_desc":
                    temas = temas.OrderByDescending(t => t.Materia.Nome);
                    break;
                case "Date":
                    temas = temas.OrderBy(t => t.DataCriacao);
                    break;
                case "date_desc":
                    temas = temas.OrderByDescending(t => t.DataCriacao);
                    break;
                case "tema_desc":
                    temas = temas.OrderByDescending(t => t.Nome);
                    break;
                default:
                    temas = temas.OrderBy(t => t.Nome);
                    break;
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                temas = temas.Where(t => t.Nome.ToUpper().Contains(request.Search.ToUpper()));
            }

            return temas;
        }

        [HttpGet]
        public async Task<FileResult> GerarRelatorioExcel(string ordemClassificacao, string search)
        {
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

                var request = new FiltroRequest
                {
                    OrdemClassificacao = ordemClassificacao,
                    Search = search
                };

                var temas = BuscarTemasExportacao(request);

                //Valores
                var r = 2;
                foreach (var tema in await temas.ToListAsync())
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

        public async Task<ActionResult> GerarRelatorioPDF(string ordemClassificacao, string search)
        {
            //RazorPDF2
            var request = new FiltroRequest
            {
                OrdemClassificacao = ordemClassificacao,
                Search = search
            };

            var temas = BuscarTemasExportacao(request);

            return new PdfActionResult("TemasReport", await temas.ToListAsync())
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
