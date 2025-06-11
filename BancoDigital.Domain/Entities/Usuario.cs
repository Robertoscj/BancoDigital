using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BancoDigital.Domain.Entities.Enums;

namespace BancoDigital.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public PerfilUsuario Perfil { get; set; }

    }
}