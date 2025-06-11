using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoDigital.Application.Requests
{
    public class ClienteRequest
    {
        public string Nome { get; set; } 
        public string Documento { get; set; } 
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
}