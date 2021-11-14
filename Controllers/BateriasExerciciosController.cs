using AutoMapper;
using ExcelDataReader;
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
    public class BateriasExerciciosController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: BateriaExercicios
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
            ViewBag.ParametroClassificacaoMateria = ordemClassificacao == "materia" ? "materia_desc" : "materia";
            ViewBag.ParametroClassificacaoTema = ordemClassificacao == "tema" ? "tema_desc" : "tema";
            ViewBag.ParametroClassificacaoQuantidadeExercicios = ordemClassificacao == "quantidade_exercicios" ? "quantidade_exercicios_desc" : "quantidade_exercicios";
            ViewBag.ParametroClassificacaoQuantidadeAcertos = ordemClassificacao == "quantidade_acertos" ? "quantidade_acertos_desc" : "quantidade_acertos";
            ViewBag.ParametroClassificacaoAproveitamento = ordemClassificacao == "aproveitamento" ? "aproveitamento_desc" : "aproveitamento";
            ViewBag.ParametroClassificacaoData = ordemClassificacao == "Date" ? "date_desc" : "Date";

            var paginatedList = await BuscarBateriasPaginacao(userId, request);

            return View(paginatedList);
        }

        public async Task<PaginatedList<BateriaExercicio>> BuscarBateriasPaginacao(int userId, FiltroRequest request)
        {
            IQueryable<BateriaExercicio> bateriasExercicios = BuscarBateriasExercicios(userId, request);

            int tamanhoPagina = 100;

            return await PaginatedList<BateriaExercicio>.CreateAsync(bateriasExercicios, request.NumeroPagina ?? 1, tamanhoPagina);
        }

        private IQueryable<BateriaExercicio> BuscarBateriasExercicios(int userId, FiltroRequest request)
        {
            var bateriasExercicios = db.BateriasExercicios
                                     .Include(a => a.Tema.Materia)
                                     .Where(a => a.Tema.Materia.UsuarioId == userId);

            switch (request.OrdemClassificacao)
            {
                case "materia":
                    bateriasExercicios = bateriasExercicios.OrderBy(b => b.Tema.Materia.Nome);
                    break;
                case "materia_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(b => b.Tema.Materia.Nome);
                    break;
                case "tema":
                    bateriasExercicios = bateriasExercicios.OrderBy(b => b.Tema.Nome);
                    break;
                case "tema_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(b => b.Tema.Nome);
                    break;
                case "quantidade_exercicios":
                    bateriasExercicios = bateriasExercicios.OrderBy(b => b.QuantidadeExercicios);
                    break;
                case "quantidade_exercicios_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(b => b.QuantidadeExercicios);
                    break;
                case "quantidade_acertos":
                    bateriasExercicios = bateriasExercicios.OrderBy(b => b.QuantidadeAcertos);
                    break;
                case "quantidade_acertos_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(b => b.QuantidadeAcertos);
                    break;
                case "aproveitamento":
                    bateriasExercicios = bateriasExercicios.OrderBy(b => b.Aproveitamento);
                    break;
                case "aproveitamento_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(b => b.Aproveitamento);
                    break;
                case "Date":
                    bateriasExercicios = bateriasExercicios.OrderBy(a => a.DataCriacao);
                    break;
                case "date_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(a => a.DataCriacao);
                    break;
                case "descricao_desc":
                    bateriasExercicios = bateriasExercicios.OrderByDescending(a => a.Descricao);
                    break;
                default:
                    bateriasExercicios = bateriasExercicios.OrderBy(a => a.Descricao);
                    break;
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                bateriasExercicios = bateriasExercicios.Where(e => e.Descricao.ToUpper().Contains(request.Search.ToUpper()));
            }

            return bateriasExercicios;
        }

        // GET: BateriaExercicios/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BateriaExercicio bateriaExercicio = await db.BateriasExercicios
                                                      .Include(b => b.Tema.Materia)
                                                      .FirstOrDefaultAsync(b => b.AtividadeId == id);

            if (bateriaExercicio == null)
            {
                return HttpNotFound();
            }

            BateriaExercicioViewModel viewModel = mapper.Map<BateriaExercicioViewModel>(bateriaExercicio);

            return View(viewModel);
        }

        // GET: BateriaExercicios/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId<int>();

            var materias = db.Materias
                           .Where(m => m.UsuarioId == userId);

            BateriaExercicioViewModel viewModel = new BateriaExercicioViewModel
            {
                Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome"),
                Temas = new SelectList(Enumerable.Empty<SelectListItem>())
            };

            return View(viewModel);
        }

        // POST: BateriaExercicios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MateriaId,TemaId,Descricao,QuantidadeExercicios,QuantidadeAcertos")] BateriaExercicioViewModel viewModel)
        {
            var userId = User.Identity.GetUserId<int>();

            BateriaExercicio bateriaExercicio = mapper.Map<BateriaExercicio>(viewModel);

            if (ModelState.IsValid)
            {
                bateriaExercicio.CalcularAproveitamento();

                db.BateriasExercicios.Add(bateriaExercicio);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);
            var temas = db.Temas
                        .Include(t => t.Materia.Usuario)
                        .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == viewModel.MateriaId);

            viewModel.Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", viewModel.MateriaId);
            viewModel.Temas = new SelectList(await temas.ToListAsync(), "TemaId", "Nome", viewModel.TemaId);

            return View(viewModel);
        }

        // GET: BateriaExercicios/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BateriaExercicio bateriaExercicio = await db.BateriasExercicios
                                                      .Include(b => b.Tema.Materia)
                                                      .FirstOrDefaultAsync(b => b.AtividadeId == id);

            if (bateriaExercicio == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);
            var temas = db.Temas
                        .Include(t => t.Materia.Usuario)
                        .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == bateriaExercicio.Tema.MateriaId);

            BateriaExercicioViewModel viewModel =
                mapper.Map<BateriaExercicioViewModel>(bateriaExercicio);
            viewModel.MateriaId = bateriaExercicio.Tema.MateriaId;

            viewModel.Materias =
                new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", bateriaExercicio.Tema.MateriaId);

            viewModel.Temas =
                new SelectList(await temas.ToListAsync(), "TemaId", "Nome", bateriaExercicio.TemaId);

            return View(viewModel);
        }

        // POST: BateriaExercicios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,Descricao,MateriaId,TemaId,QuantidadeExercicios,QuantidadeAcertos,DataCriacao,UsuarioCriacao")] BateriaExercicioViewModel viewModel)
        {
            BateriaExercicio bateriaExercicio = mapper.Map<BateriaExercicio>(viewModel);

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

            BateriaExercicio bateriaExercicio =
                await db.BateriasExercicios
                      .Include(b => b.Tema.Materia)
                      .FirstOrDefaultAsync(b => b.AtividadeId == id);

            if (bateriaExercicio == null)
            {
                return HttpNotFound();
            }

            BateriaExercicioViewModel viewModel =
                mapper.Map<BateriaExercicioViewModel>(bateriaExercicio);

            return View(viewModel);
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
        public async Task<ActionResult> InserirPorExcel()
        {
            var userId = User.Identity.GetUserId<int>();

            if (Request.Files.Count > 0)
            {
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
                }

                excelReader.Close();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.BateriasExercicios.AddRange(exercicios);
                    await db.MyBulkInsertAsync(exercicios);

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Where(m => m.UsuarioId == userId);

            BateriaExercicioViewModel viewModel = new BateriaExercicioViewModel
            {
                Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome"),
                Temas = new SelectList(Enumerable.Empty<SelectListItem>())
            };

            ModelState.AddModelError("", "O arquivo é obrigatório.");

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

                List<BateriaExercicio> bateriasExercicios = new List<BateriaExercicio>();

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
                            var videoAulaId = excelReader.GetDouble(0);

                            var bateriaExercicio = await db.BateriasExercicios
                                                   .Include(a => a.Tema.Materia.Usuario)
                                                   .FirstAsync(a => a.Tema.Materia.UsuarioId == userId && a.AtividadeId == videoAulaId);
                            bateriaExercicio.Descricao = excelReader.GetString(1);
                            bateriaExercicio.QuantidadeExercicios = int.Parse(excelReader.GetValue(2).ToString());
                            bateriaExercicio.QuantidadeAcertos = int.Parse(excelReader.GetValue(3).ToString());
                            bateriaExercicio.CalcularAproveitamento();

                            db.Entry(bateriaExercicio).State = EntityState.Modified;
                            bateriasExercicios.Add(bateriaExercicio);
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
                    await db.MyBulkUpdateAsync(bateriasExercicios);

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
                excelPackage.Workbook.Properties.Title = "Relatório-Exercícios";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Exercícios");

                //Títulos
                var i = 1;
                var titulos = new [] { "Descrição", "Quantidade de exercícios", "Quantidade de acertos", "Aproveitamento", "Matéria", "Tema" };

                sheet.Cells["A1:F1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells["A1:F1"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#5794d8"));
                sheet.Cells["A1:F1"].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FFF"));

                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var request = new FiltroRequest
                {
                    OrdemClassificacao = ordemClassificacao,
                    Search = search
                };

                var bateriasExercicios = BuscarBateriasExercicios(userId, request);

                //Valores
                var r = 2;
                foreach (var exercicio in await bateriasExercicios.ToListAsync())
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

        public async Task<ActionResult> GerarRelatorioPDF(string ordemClassificacao, string search)
        {
            //RazorPDF2

            var userId = User.Identity.GetUserId<int>();

            var request = new FiltroRequest
            {
                OrdemClassificacao = ordemClassificacao,
                Search = search
            };

            var bateriasExercicios = BuscarBateriasExercicios(userId, request);

            return new PdfActionResult("BateriasExerciciosReport", await bateriasExercicios.ToListAsync())
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