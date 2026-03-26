using Microsoft.AspNetCore.Mvc;
using SistemaContratos.Data;
using SistemaContratos.Models;
using System.Diagnostics;
using System.Globalization;

namespace SistemaContratos.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _banco;

        public HomeController(AppDbContext banco)
        {
            _banco = banco;
        }

        // Tela do formulario para importação 
        public IActionResult Index()
        {
            return View();
        }

        // Tela de consulta de contratos, pega os contratos e envia para a tela 
        public IActionResult Contratos()
        {
            var listaDeContratos = _banco.Contratos.ToList();
            return View(listaDeContratos);
        }

        // Tela de consulta por cliente, calcula o valor total, o atraso em dias e envia para a tela
        public IActionResult Clientes()
        {
            var listaDeContratos = _banco.Contratos.ToList();
            var dataAtual = DateTime.Today;

            var resumoClientes = listaDeContratos
                .GroupBy(c => c.NomeCliente)
                .Select(grupo => new ResumoCliente
                {
                    NomeCliente = grupo.Key,
                    ValorTotal = grupo.Sum(c => c.Valor),
                    MaiorAtrasoDias = grupo.Max(c => (dataAtual - c.DataVencimento).Days > 0 ? (dataAtual - c.DataVencimento).Days : 0)
                })
                .OrderByDescending(r => r.ValorTotal)
                .ToList();

            return View(resumoClientes);
        }

        // Recebe o arquivo, valida e realiza a importação
        [HttpPost]
        public IActionResult Importar(IFormFile arquivoContratos)
        {
            // Para deixar acentos 
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (arquivoContratos == null || arquivoContratos.Length == 0)
            {
                TempData["Erro"] = "Nenhum arquivo selecionado.";
                return RedirectToAction("Index");
            }

            // Limpa o banco de dados antes da importação
            _banco.Contratos.RemoveRange(_banco.Contratos);
            _banco.SaveChanges();

            int linhasImportadas = 0;

            // Abre o arquivo .CSV recebido com acentos 
            using (var leitor = new StreamReader(arquivoContratos.OpenReadStream(), System.Text.Encoding.GetEncoding("iso-8859-1")))
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

                    if (colunas.Length < 6)
                    {
                        continue;
                    }

                    try
                    {
                        // Define a cultura pra pb-br pra converter a data e o valor
                        var cultura = new CultureInfo("pt-BR");

                        // Cria o objeto Contrato com os dados da linha
                        var contrato = new Contrato
                        {
                            NomeCliente = colunas[0].Trim(),
                            CPF = colunas[1].Trim(),
                            NumContrato = colunas[2].Trim(),
                            Produto = colunas[3].Trim(),
                            DataVencimento = Convert.ToDateTime(colunas[4].Trim(), cultura),
                            Valor = Convert.ToDecimal(colunas[5].Trim(), cultura)
                        };
                        _banco.Contratos.Add(contrato);
                        linhasImportadas++;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                _banco.SaveChanges();
            }
            if (linhasImportadas > 0)
            {
                TempData["Sucesso"] = $"{linhasImportadas} contratos importados, consultas liberadas.";
            }
            else
            {
                TempData["Erro"] = "O arquivo está em formato inválido, está vazio ou não possui as colunas obrigatórias.";
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
