using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaContratos.Data;
using SistemaContratos.Models;
using System.Diagnostics;
using System.IO;
using System;

namespace SistemaContratos.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _banco;

        public HomeController(AppDbContext banco)
        {
            _banco = banco;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Recebe o arquivo, valida e realiza a importação
        [HttpPost]
        public IActionResult Importar(IFormFile arquivoContratos)
        {
            if (arquivoContratos == null || arquivoContratos.Length == 0)
            {
                return RedirectToAction("Index");
            }

            // Abre o arquivo .CSV recebido
            using (var leitor = new StreamReader(arquivoContratos.OpenReadStream()))
            {
                // Pula o cabeçalho
                leitor.ReadLine();

                // Lê linha por linha 
                while (!leitor.EndOfStream)
                {
                    var linha = leitor.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                    {
                        continue;
                    }

                    // Divide a linha onde tem ponto e vírgula
                    var colunas = linha.Split(';');

                    // Cria o objeto Contrato com os dados da linha
                    var contrato = new Contrato
                    {
                        NomeCliente = colunas[0],
                        Valor = Convert.ToDecimal(colunas[1]),
                        DataVencimento = Convert.ToDateTime(colunas[2])
                    };

                    // Adiciona e depois salva no banco de dados
                    _banco.Contratos.Add(contrato);
                }
                _banco.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
