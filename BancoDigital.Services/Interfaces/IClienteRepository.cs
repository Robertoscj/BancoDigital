using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BancoDigital.Domain.Entities;

namespace BancoDigital.Services.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> ObterPorIdAsync(int id);
        Task<Cliente?> ObterPorDocumentoAsync(string documento);
        Task<IEnumerable<Cliente>> ListarTodosAsync();
        Task<int> CriarAsync(Cliente cliente);
        Task AtualizarAsync(Cliente cliente);
        Task DeletarAsync(int id);
    }
}