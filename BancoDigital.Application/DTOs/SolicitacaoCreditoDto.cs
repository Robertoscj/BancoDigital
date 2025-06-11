using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoDigital.Application.DTOs
{
    public class SolicitacaoCreditoDto
    {
         public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime DataSolicitacao { get; set; } 
        public string Status { get; set; } = null!;
        public int? Score { get; set; }
        public string? Justificativa { get; set; }
        public DateTime? DataResposta { get; set; }
    }
}