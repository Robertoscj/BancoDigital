using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoDigital.Domain.Entities
{
    public class SolicitacaoCredito
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime DataSolicitacao { get; set; } = DateTime.Now;
        public StatusSolicitacao Status { get; set; } = StatusSolicitacao.EmAndamento;
        public int? Score { get; set; }
        public string? Justificativa { get; set; }
        public DateTime? DataResposta { get; set; }
    }
}