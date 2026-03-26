using System.ComponentModel.DataAnnotations;

namespace SistemaContratos.Models
{
    public class Contrato
    {
        [Key]
        public int Id { get; set; }

        public string NomeCliente {  get; set; }

        public decimal Valor { get; set; }

        public DateTime DataVencimento {  get; set; }
    }
}
