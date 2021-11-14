using AutoMapper;
using ExcelDataReader;
using Humanizer;
using MeuCantinhoDeEstudos3.Extensions;
using MeuCantinhoDeEstudos3.Mappers;
using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.ViewModels;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RazorPDF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class AtividadesController : System.Web.Mvc.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: Atividades
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
            ViewBag.ParametroClassificacaoDescricao = string.IsNullOrEmpty(ordemClassificacao) ? "descricao_desc" : "";
            ViewBag.ParametroClassificacaoData = ordemClassificacao == "Date" ? "date_desc" : "Date";

            var paginatedList = await PaginarAtividades(userId, request);

            return View(paginatedList);
        }

        private async Task<PaginatedList<Atividade>> PaginarAtividades(int userId, FiltroRequest request)
        {
            IQueryable<Atividade> atividades = BuscarAtividades(userId, request);

            int tamanhoPagina = 100;

            return await PaginatedList<Atividade>.CreateAsync(atividades, request.NumeroPagina ?? 1, tamanhoPagina);
        }

        private IQueryable<Atividade> BuscarAtividades(int userId, FiltroRequest request)
        {
            var atividades = db.Atividades
                            .Include(a => a.Tema.Materia)
                            .Where(a => a.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(request.Search))
            {
                atividades = atividades.Where(a => a.Descricao.ToUpper().Contains(request.Search.ToUpper()));
            }

            switch (request.OrdemClassificacao)
            {
                case "Date":
                    atividades = atividades.OrderBy(a => a.DataCriacao);
                    break;
                case "date_desc":
                    atividades = atividades.OrderByDescending(a => a.DataCriacao);
                    break;
                case "descricao_desc":
                    atividades = atividades.OrderByDescending(a => a.Descricao);
                    break;
                default:
                    atividades = atividades.OrderBy(a => a.Descricao);
                    break;
            }

            return atividades;
        }

        // GET: Atividades/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Atividade atividade = await db.Atividades
                                        .Include(a => a.Tema.Materia)
                                        .FirstOrDefaultAsync(a => a.AtividadeId == id);
            
            if (atividade == null)
            {
                return HttpNotFound();
            
            }

            var data = DateTime.Now.Humanize();

            AtividadeViewModel viewModel = mapper.Map<AtividadeViewModel>(atividade);

            return View(viewModel);
        }

        // GET: Atividades/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId<int>();

            AtividadeViewModel viewModel = new AtividadeViewModel
            {
                Materias = new SelectList(await db.Materias
                                .Where(m => m.UsuarioId == userId).ToListAsync(),
                                "MateriaId",
                                "Nome"),
                Temas = new SelectList(Enumerable.Empty<SelectListItem>())
            };

            return View(viewModel);
        }

        // POST: Atividades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TemaId,Descricao")] AtividadeViewModel viewModel)
        {
            var userId = User.Identity.GetUserId<int>();

            Atividade atividade = mapper.Map<Atividade>(viewModel);

            if (ModelState.IsValid)
            {
                db.Atividades.Add(atividade);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            ViewBag.MateriaId = new SelectList(materias, "MateriaId", "Nome");
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

            Atividade atividade = await db.Atividades
                                        .Include(a => a.Tema.Materia)
                                        .FirstOrDefaultAsync(a => a.AtividadeId == id);

            if (atividade == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            var temas = db.Temas
                        .Include(t => t.Materia)
                        .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == atividade.Tema.MateriaId);

            AtividadeViewModel viewModel = mapper.Map<AtividadeViewModel>(atividade);
            viewModel.MateriaId = atividade.Tema.MateriaId;
            viewModel.Materias = new SelectList(materias, "MateriaId", "Nome");
            viewModel.Temas = new SelectList(temas, "TemaId", "Nome", atividade.TemaId);

            return View(viewModel);
        }

        // POST: Atividades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,TemaId,Descricao,DataCriacao,UsuarioCriacao")] AtividadeViewModel viewModel)
        {
            Atividade atividade = mapper.Map<Atividade>(viewModel);

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

            Atividade atividade = await db.Atividades
                                        .Include(a => a.Tema.Materia)
                                        .FirstOrDefaultAsync(a => a.AtividadeId == id);
            
            if (atividade == null)
            {
                return HttpNotFound();
            }

            AtividadeViewModel viewModel = mapper.Map<AtividadeViewModel>(atividade);

            return View(viewModel);
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
        public async Task<ActionResult> InserirPorExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            
            if (Request.Files.Count > 0)
            {
                var postedFile = Request.Files[0];

                List<Atividade> atividades = new List<Atividade>();

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
                    db.Atividades.AddRange(atividades);
                    await db.MyBulkInsertAsync(atividades);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            AtividadeViewModel viewModel = new AtividadeViewModel
            {
                Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome"),
                Temas = new SelectList(Enumerable.Empty<SelectListItem>())
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

                List<Atividade> atividades = new List<Atividade>();

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
                        try
                        {
                            var atividadeId = excelReader.GetDouble(0);

                            var atividade = await db.Atividades
                                            .Include(a => a.Tema.Materia.Usuario)
                                            .FirstAsync(a => a.Tema.Materia.UsuarioId == userId && a.AtividadeId == atividadeId);
                            atividade.Descricao = excelReader.GetString(1);

                            db.Entry(atividade).State = EntityState.Modified;
                            atividades.Add(atividade);
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
                    await db.MyBulkUpdateAsync(atividades);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public async Task<FileResult> GerarRelatorioExcel(string ordemClassificacao, string search)
        {
            var userId = User.Identity.GetUserId<int>();

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Meu Cantinho de Estudos";
                excelPackage.Workbook.Properties.Title = "Relatório-Atividades";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Atividades");

                //Títulos
                var i = 1;
                var titulos = new [] { "Descrição", "Matéria", "Tema" };

                sheet.Cells["A1:C1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#5794d8"));
                sheet.Cells["A1:C1"].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FFF"));

                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var request = new FiltroRequest
                {
                    OrdemClassificacao = ordemClassificacao,
                    Search = search
                };

                var atividades = BuscarAtividades(userId, request);

                //Valores
                var r = 2;
                foreach (var atividade in await atividades.ToListAsync())
                {
                    sheet.Cells[r, 1].Value = atividade.Descricao;
                    sheet.Cells[r, 2].Value = atividade.Tema.Materia.Nome;
                    sheet.Cells[r++, 3].Value = atividade.Tema.Nome;
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

            var userId = User.Identity.GetUserId<int>();

            var request = new FiltroRequest
            {
                OrdemClassificacao = ordemClassificacao,
                Search = search
            };

            IQueryable<Atividade> atividades = BuscarAtividades(userId, request);

            return new PdfActionResult("AtividadesReport", await atividades.ToListAsync())
            {
                FileDownloadName = "Relatório-Atividades.pdf"
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