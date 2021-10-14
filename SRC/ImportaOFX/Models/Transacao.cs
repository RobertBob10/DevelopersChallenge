using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ImportaOFX.Models
{
    [Table("Transacao")]
    public class Transacao
    {
        [Key, Column("Id")]
        [Display(Name = "Código")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Tipo")]
        [Display(Name = "Tipo")]
        public string TipoTransacao { get; set; }

        [Column("Data")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Transação")]
        public DateTime DataTransacao { get; set; }

        [Column("Valor")]
        [DataType(DataType.Currency)]
        [Display(Name = "Valor da Transação")]
        public decimal ValorTransacao { get; set; }

        [Column("Descricao")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Column("Observacao")]
        [Display(Name = "Observacao")]
        public string Observacao { get; set; }
    }
}
