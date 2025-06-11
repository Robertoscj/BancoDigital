
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