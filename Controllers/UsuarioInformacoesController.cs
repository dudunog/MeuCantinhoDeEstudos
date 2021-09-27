using EntityFramework.Extensions;
using MeuCantinhoDeEstudos3.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Z.BulkOperations;

namespace MeuCantinhoDeEstudos3.Controllers
{
    [Authorize]
    public class UsuarioInformacoesController : Controller
    {
        private MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

        // GET: UsuarioInformacoes/Create
        public async Task<ActionResult> Create()
        {
            //ViewData["UsuarioId"]
            var userId = User.Identity.GetUserId<int>();

            UsuarioInformacoes usuarioInformacoes = await db.UsuarioInformacoes.FindAsync(userId);

            if (usuarioInformacoes != null)
            {
                return RedirectToAction("Edit");
            }

            return View();
        }

        // POST: /UsuarioInformacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Pais,Estado,Cidade,CEP,Logradouro,Numero")] UsuarioInformacoes usuarioInformacoes)
        {
            if (ModelState.IsValid)
            {
                usuarioInformacoes.UsuarioId = User.Identity.GetUserId<int>();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    db.Entry(usuarioInformacoes).State = EntityState.Added;
                    
                    List<AuditEntry> auditEntries = new List<AuditEntry>();

                    db.BulkSaveChanges(options =>
                    {
                        options.UseAudit = true;
                        options.AuditEntries = auditEntries;
                    });

                    await SaveUsuarioInformacoesAuditChanges(auditEntries, User.Identity.GetUserId<int>());

                    scope.Complete();
                }

                return RedirectToAction("Edit");
            }

            return RedirectToAction("Edit");
        }

        // GET: /UsuarioInformacoes/Edit
        public async Task<ActionResult> Edit()
        {
            var userId = User.Identity.GetUserId<int>();

            UsuarioInformacoes usuarioInformacoes = await db.UsuarioInformacoes.FindAsync(userId);

            if (usuarioInformacoes == null)
            {
                return HttpNotFound();
            }

            return View(usuarioInformacoes);
        }

        // POST: /UsuarioInformacoes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Pais,Estado,Cidade,CEP,Logradouro,Numero")] UsuarioInformacoes usuarioInformacoes)
        {
            if (ModelState.IsValid)
            {
                usuarioInformacoes.UsuarioId = User.Identity.GetUserId<int>();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {                    
                    db.Entry(usuarioInformacoes).State = EntityState.Modified;
                    
                    List<AuditEntry> auditEntries = new List<AuditEntry>();

                    db.BulkSaveChanges(options =>
                    {
                        options.UseAudit = true;
                        options.AuditEntries = auditEntries;
                    });

                    await SaveUsuarioInformacoesAuditChanges(auditEntries, User.Identity.GetUserId<int>());
                    
                    scope.Complete();
                }

                return RedirectToAction("Edit");
            }

            return RedirectToAction("Edit");
        }

        private static async Task SaveUsuarioInformacoesAuditChanges(List<AuditEntry> auditEntries, int userId)
        {
            MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();

            List<UsuarioInformacoesLog> auditLogs = new List<UsuarioInformacoesLog>();

            List<UsuarioInformacoesLogValores> auditLogsValues = new List<UsuarioInformacoesLogValores>();

            foreach (var auditEntry in auditEntries)
            {
                UsuarioInformacoesLog usuarioLog = new UsuarioInformacoesLog()
                {
                    UsuarioId = userId,
                    Action = auditEntry.Action.ToString(),
                    NomeTabela = auditEntry.TableName,
                    Data = auditEntry.Date,
                    Valores = new List<UsuarioInformacoesLogValores>(),
                };

                auditLogs.Add(usuarioLog);

                await db.BulkInsertAsync(auditLogs);

                foreach (var value in auditEntry.Values)
                {
                    var usuarioLogValue = new UsuarioInformacoesLogValores()
                    {
                        UsuarioInformacoesLogId = usuarioLog.UsuarioInformacoesLogId,
                        NomePropriedade = value.ColumnName,
                        ValorAntigo = value.OldValue != null ? value.OldValue.ToString() : null,
                        ValorNovo = value.NewValue != null ? value.NewValue.ToString() : null,
                    };

                    auditLogsValues.Add(usuarioLogValue);
                }
            }

            await db.BulkInsertAsync(auditLogsValues, options => options.AutoMapOutputDirection = false);
        }
    }
}