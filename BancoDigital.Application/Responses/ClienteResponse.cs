using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoDigital.Application.Responses
{
    public class ClienteResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Documento { get; set; } = null!;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public bool CadastroCompleto { get; set; }
    }
}