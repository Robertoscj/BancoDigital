using BancoDigital.Domain.Entities;
using BancoDigital.Services.Interfaces;

namespace BancoDigital.Services.Repositories
{
    public class ClienteRepository : IClienteRepository
    {

         public Task<Cliente?> ObterPorIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> CriarAsync(Cliente cliente)
        {
            throw new NotImplementedException();
        }


        public Task<Cliente?> ObterPorDocumentoAsync(string documento)
        {
            throw new NotImplementedException();
        }


         public Task<IEnumerable<Cliente>> ListarTodosAsync()
        {
            throw new NotImplementedException();
        }

        public Task AtualizarAsync(Cliente cliente)
        {
            throw new NotImplementedException();
        }

        

        public Task DeletarAsync(int id)
        {
            throw new NotImplementedException();
        }

        
        
       
    }
}