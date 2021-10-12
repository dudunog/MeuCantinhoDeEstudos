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
    public class AtividadesController : System.Web.Mvc.Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        private readonly IMapper mapper = AutoMapperConfig.Mapper;

        // GET: Atividades
        public async Task<ActionResult> Index(string search)
        {
            var userId = User.Identity.GetUserId<int>();

            ViewBag.CurrentSearch = search;

            var atividades = db.Atividades
                             .Include(a => a.Tema.Materia)
                             .Where(a => a.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                atividades = atividades.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            IEnumerable<AtividadeViewModel> viewModels =
                mapper.Map<List<AtividadeViewModel>>(await atividades.ToListAsync());

            return View(viewModels);
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

            AtividadeViewModel viewModel = mapper.Map<AtividadeViewModel>(atividade);

            return View(viewModel);
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
        public async Task<ActionResult> Edit([Bind(Include = "AtividadeId,TemaId,Descricao")] AtividadeViewModel viewModel)
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
                db.Atividades.AddRange(atividades);
                await db.BulkSaveChangesAsync();

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
                excelPackage.Workbook.Properties.Title = "Relatório-Atividades";

                //Criação da planilha
                var sheet = excelPackage.Workbook.Worksheets.Add("Atividades");

                //Títulos
                var i = 1;
                var titulos = new [] { "Descrição", "Matéria", "Tema" };
                foreach (var titulo in titulos)
                {
                    sheet.Cells[1, i++].Value = titulo;
                }

                var atividades = db.Atividades
                                .Include(a => a.Tema.Materia)
                                .Where(a => a.Tema.Materia.UsuarioId == userId);

                if (!string.IsNullOrEmpty(search))
                {
                    atividades = atividades.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
                }

                //Valores
                var r = 2;
                foreach (var atividade in atividades)
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

        public ActionResult GerarRelatorioPDF(string search)
        {
            //RazorPDF2
            var userId = User.Identity.GetUserId<int>();

            var atividades = db.Atividades
                             .Include(a => a.Tema.Materia)
                             .Where(a => a.Tema.Materia.UsuarioId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                atividades = atividades.Where(a => a.Descricao.ToUpper().Contains(search.ToUpper()));
            }

            return new PdfActionResult("AtividadesReport", atividades.ToList())
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