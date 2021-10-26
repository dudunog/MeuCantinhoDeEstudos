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
    public class VideoAulasController : System.Web.Mvc.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: VideoAulas
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

            var paginatedList = await BuscarVideoAulas(userId, request);

            return View(paginatedList);
        }

        private async Task<PaginatedList<VideoAula>> BuscarVideoAulas(int userId, FiltroRequest request)
        {
            var videoAulas = db.VideoAulas
                             .Include(v => v.Tema.Materia)
                             .Where(v => v.Tema.Materia.UsuarioId == userId);

            switch (request.OrdemClassificacao)
            {
                case "Date":
                    videoAulas = videoAulas.OrderBy(a => a.DataCriacao);
                    break;
                case "date_desc":
                    videoAulas = videoAulas.OrderByDescending(a => a.DataCriacao);
                    break;
                case "descricao_desc":
                    videoAulas = videoAulas.OrderByDescending(a => a.Descricao);
                    break;
                default:
                    videoAulas = videoAulas.OrderBy(a => a.Descricao);
                    break;
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                videoAulas = videoAulas.Where(v => v.Descricao.ToUpper().Contains(request.Search.ToUpper()));
            }

            int tamanhoPagina = 100;
            return await PaginatedList<VideoAula>.CreateAsync(videoAulas, request.NumeroPagina ?? 1, tamanhoPagina);
        }

        // GET: VideoAulas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            VideoAula videoAula = await db.VideoAulas
                                        .Include(v => v.Tema.Materia)
                                        .FirstOrDefaultAsync(v => v.AtividadeId == id);
            
            if (videoAula == null)
            {
                return HttpNotFound();
            }

            VideoAulaViewModel viewModel = mapper.Map<VideoAulaViewModel>(videoAula);

            return View(viewModel);
        }

        // GET: VideoAulas/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId<int>();

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            VideoAulaViewModel viewModel = new VideoAulaViewModel
            {
                Materias = new SelectList(await materias.ToListAsync(), "MateriaId", "Nome"),
                Temas = new SelectList(Enumerable.Empty<SelectListItem>())
            };

            return View(viewModel);
        }

        // POST: VideoAulas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MateriaId,TemaId,Descricao,LinkVideo")] VideoAulaViewModel viewModel)
        {
            var userId = User.Identity.GetUserId<int>();

            VideoAula videoAula = mapper.Map<VideoAula>(viewModel);

            if (ModelState.IsValid)
            {
                db.VideoAulas.Add(videoAula);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);
            var temas = db.Temas
                        .Include(t => t.Materia.Usuario)
                        .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == viewModel.MateriaId);

            viewModel.Materias =
                new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", viewModel.MateriaId);
            viewModel.Temas =
                new SelectList(await temas.ToListAsync(), "TemaId", "Nome", viewModel.TemaId);

            return View(viewModel);
        }

        // GET: VideoAulas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            VideoAula videoAula = await db.VideoAulas
                                        .Include(v => v.Tema.Materia)
                                        .FirstOrDefaultAsync(v => v.AtividadeId == id);
            
            if (videoAula == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);
            var temas = db.Temas
                        .Include(t => t.Materia)
                        .Where(t => t.Materia.UsuarioId == userId && t.MateriaId == videoAula.Tema.MateriaId);

            VideoAulaViewModel viewModel = mapper.Map<VideoAulaViewModel>(videoAula);
            viewModel.MateriaId = videoAula.Tema.MateriaId;
            viewModel.Materias = 
                new SelectList(await materias.ToListAsync(), "MateriaId", "Nome", videoAula.Tema.MateriaId);
            viewModel.Temas = 
                new SelectList(await temas.ToListAsync(), "TemaId", "Nome", videoAula.TemaId);

            return View(viewModel);
        }

        // POST: VideoAulas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,TemaId,Descricao,LinkVideo")] VideoAulaViewModel viewModel)
        {
            VideoAula videoAula = mapper.Map<VideoAula>(viewModel);

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

            VideoAula videoAula = await db.VideoAulas
                                        .Include(v => v.Tema.Materia)
                                        .FirstOrDefaultAsync(v => v.AtividadeId == id);
            
            if (videoAula == null)
            {
                return HttpNotFound();
            }

            VideoAulaViewModel viewModel = mapper.Map<VideoAulaViewModel>(videoAula);

            return View(viewModel);
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

        [HttpPost]
        public async Task<ActionResult> ImportarExcel()
        {
            var userId = User.Identity.GetUserId<int>();
            
            if (Request.Files.Count > 0)
            {
                var postedFile = Request.Files[0];
                var videoAulas = new List<VideoAula>();

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
                        videoAulas.Add(new VideoAula
                        {
                            TemaId = int.Parse(excelReader.GetValue(0).ToString()),
                            Descricao = excelReader.GetString(1),
                            LinkVideo = excelReader.GetString(2),
                            DataCriacao = DateTime.Now,
                            UsuarioCriacao = User.Identity.GetUserName(),
                        });
                    }
                }

                excelReader.Close();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.VideoAulas.AddRange(videoAulas);
                    await db.BulkSaveChangesAsync();

                    scope.Complete();
                }

                return RedirectToAction("Index");
            }

            var materias = db.Materias
                           .Include(m => m.Usuario)
                           .Where(m => m.UsuarioId == userId);

            VideoAulaViewModel viewModel = new VideoAulaViewModel
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
                excelPackage.Workbook.Properties.Title = "Relatório-Vídeoaulas";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Vídeoaulas");

                //Títulos
                var i = 1;
                var titulos = new String[] { "Descrição", "Link do vídeo", "Matéria", "Tema" };
                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var videoaulas = db.VideoAulas
                                .Include(a => a.Tema.Materia)
                                .Where(a => a.Tema.Materia.UsuarioId == userId);

                if (!string.IsNullOrEmpty(search))
                {
                    videoaulas = videoaulas.Where(v => v.Descricao.ToUpper().Contains(search.ToUpper()));
                }

                //Valores
                var r = 2;
                foreach (var videoaula in videoaulas)
                {
                    sheet.Cells[r, 1].Value = videoaula.Descricao;
                    sheet.Cells[r, 2].Value = videoaula.LinkVideo;
                    sheet.Cells[r, 3].Value = videoaula.Tema.Materia.Nome;
                    sheet.Cells[r++, 4].Value = videoaula.Tema.Nome;
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

            var videoaulas = db.VideoAulas
                                .Include(a => a.Tema.Materia)
                                .Where(a => a.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                videoaulas = videoaulas.Where(v => v.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            return new PdfActionResult("VideoAulasReport", videoaulas.ToList())
            {
                FileDownloadName = "Relatório-Vídeoaulas.pdf"
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