using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoDigital.Application.Responses
{
    public class SolicitacaoCreditoResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public int? Score { get; set; }
        public string? Justificativa { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataResposta { get; set; }
    }
}