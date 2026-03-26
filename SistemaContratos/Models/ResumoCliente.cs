namespace SistemaContratos.Models
{
    public class ResumoCliente
    {
        public string NomeCliente { get; set; }

        public decimal ValorTotal { get; set; }

        public int MaiorAtrasoDias { get; set; }
    }
}
