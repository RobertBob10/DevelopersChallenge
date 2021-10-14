using ImportaOFX.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace ImportaOFX.Controllers
{
    public class ImportacaoController : Controller
    {
        DbBdContext db;

        public ImportacaoController(DbBdContext _db)
        {
            db = _db;
        }
        public IActionResult Importar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Importar(List<IFormFile> files)
        {
            List<Transacao> transacoes = new List<Transacao>();
            Transacao transacao = new Transacao();

            //percorrer arquivos
            foreach (var formFile in files)
            {
                var extensao = Path.GetExtension(formFile.FileName);

                //verificar extensão do arquivo
                if (extensao.ToUpper() != ".OFX")
                {
                    ModelState.AddModelError("", "Tipo de Arquivo Inválido. É Permitida importação Somente de Arquivos do tipo OFX.");
                    return View();
                }

                if (formFile.Length > 0)
                {
                    using (var arquivo = new StreamReader(formFile.OpenReadStream()))
                    {
                        //percorre todas as linhas do arquivo
                        while (arquivo.Peek() >= 0) 
                        {
                            var linha = arquivo.ReadLine();
                            var inicio = linha.IndexOf(">") + 1;
                            var conteudo = linha.Substring(inicio);

                            if (linha.Contains("TRNTYPE"))
                            {
                                transacao.TipoTransacao = conteudo.Trim();
                            }
                            else if (linha.Contains("DTPOSTED"))
                            {
                                transacao.DataTransacao = Convert.ToDateTime(DateTime.ParseExact(conteudo.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy"));
                            }
                            else if (linha.Contains("TRNAMT"))
                            {
                                decimal.TryParse(conteudo.Trim().Replace('.', ','), out decimal vlrTransacao);
                                transacao.ValorTransacao = vlrTransacao;
                            }
                            else if (linha.Contains("MEMO"))
                            {
                                transacao.Descricao = conteudo.Trim();
                            }

                            if (linha.Contains("</STMTTRN>"))
                            {
                                //Verificar Transações da Lista para não permitir duplicação
                                if (!transacoes.Any(x => x.TipoTransacao == transacao.TipoTransacao && x.DataTransacao == transacao.DataTransacao 
                                    && x.ValorTransacao == transacao.ValorTransacao && x.Descricao == transacao.Descricao))
                                {
                                    transacoes.Add(new Transacao
                                    {
                                        TipoTransacao = transacao.TipoTransacao,
                                        DataTransacao = transacao.DataTransacao,
                                        ValorTransacao = transacao.ValorTransacao,
                                        Descricao = transacao.Descricao
                                    });
                                }
                            }
                        }
                    }
                }
            }

            //Verificar valores do BD para não incluir duplicações
            foreach (var item in transacoes)
            {
                if (!db.Transacoes.Any(x => x.TipoTransacao == item.TipoTransacao && x.DataTransacao == item.DataTransacao
                        && x.ValorTransacao == item.ValorTransacao && x.Descricao == item.Descricao))
                {
                    db.Transacoes.Add(item);
                }
            }

            db.SaveChanges();
            TempData["Sucesso"] = files.Count() + " Arquivo(s) Importado(s). " + transacoes.Count() + " Transacoes Salvas!";

            return View();
        }

        //Filtrar transações
        public IActionResult VisualizarTransacoes(DateTime? dataI, DateTime? dataF)
        {
            if (dataI != null && dataF != null)
            {
                return View(db.Transacoes.Where(x => x.DataTransacao >= dataI && x.DataTransacao <= dataF).ToList());
            }

            List<Transacao> transacoes = new List<Transacao>();

            return View(transacoes);
        }

        public IActionResult EditarObservacao(int? id, string observacao)
        {
            if (id == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            Transacao transacao = db.Transacoes.Find(id);
            transacao.Observacao = observacao;
            db.Entry(transacao).State = EntityState.Modified;
            db.SaveChanges();

            if (transacao == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            return RedirectToAction("VisualizarTransacoes",new { dataI = transacao.DataTransacao, dataF = transacao.DataTransacao });
        }
    }
}
