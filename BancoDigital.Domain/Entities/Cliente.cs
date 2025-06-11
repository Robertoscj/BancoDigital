using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoDigital.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty; // CPF ou CNPJ
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool CadastroCompleto { get; set; } = false;

        public ICollection<SolicitacaoCredito> Solicitacoes { get; set; } = new List<SolicitacaoCredito>();


        public bool PodeSolicitarCredito()
        {
            // Verifica se o cliente tem cadastro completo e se não possui solicitações pendentes
            return CadastroCompleto && !Solicitacoes.Any(s => s.DataSolicitacao > DateTime.Now.AddDays(-30));
        }

    }
}