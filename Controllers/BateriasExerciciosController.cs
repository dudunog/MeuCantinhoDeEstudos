using AutoMapper;
using ExcelDataReader;
using MeuCantinhoDeEstudos3.Mappers;
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

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: BateriaExercicios
        public async Task<ActionResult> Index(string ordemClassificacao, string filtroAtual, string search, int? numeroPagina)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = !string.IsNullOrEmpty(search) ? search : filtroAtual;
            ViewBag.ClassificacaoAtual = ordemClassificacao;
            ViewBag.ParametroClassificacaoDescricao = string.IsNullOrEmpty(ordemClassificacao) ? "descricao_desc" : "";
            ViewBag.ParametroClassificacaoMateria = ordemClassificacao == "materia" ? "materia_desc" : "materia";
            ViewBag.ParametroClassificacaoTema = ordemClassificacao == "tema" ? "tema_desc" : "tema";
            ViewBag.ParametroClassificacaoQuantidadeExercicios = ordemClassificacao == "quantidade_exercicios" ? "quantidade_exercicios_desc" : "quantidade_exercicios";
            ViewBag.ParametroClassificacaoQuantidadeAcertos = ordemClassificacao == "quantidade_acertos" ? "quantidade_acertos_desc" : "quantidade_acertos";
            ViewBag.ParametroClassificacaoAproveitamento = ordemClassificacao == "aproveitamento" ? "aproveitamento_desc" : "aproveitamento";
            ViewBag.ParametroClassificacaoData = ordemClassificacao == "Date" ? "date_desc" : "Date";

            if (search != null)
            {
                numeroPagina = 1;
            }
            else
            {
                search = filtroAtual;
            }

            ViewBag.FiltroAtual = search;

            var bateriasExercicios = db.BateriasExercicios
                                    .Include(b => b.Tema.Materia)
                                    .Where(b => b.Tema.Materia.UsuarioId == userId);

            switch (ordemClassificacao)
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

            if (!string.IsNullOrEmpty(search))
            {
                bateriasExercicios = bateriasExercicios.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            int tamanhoPagina = 100;
            var paginatedList = await PaginatedList<BateriaExercicio>.CreateAsync(bateriasExercicios, numeroPagina ?? 1, tamanhoPagina);

            //IEnumerable<BateriaExercicioViewModel> viewModels =
            //    mapper.Map<IEnumerable<BateriaExercicioViewModel>>(await bateriasExercicios.ToListAsync());

            return View(paginatedList);
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
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,Descricao,MateriaId,TemaId,QuantidadeExercicios,QuantidadeAcertos")] BateriaExercicioViewModel viewModel)
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
        public async Task<ActionResult> ImportarExcel()
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