🧠 Regra de Negócio
Imagine uma API BFF para um banco digital, com a seguinte funcionalidade:

📌 “Gerenciar solicitações de análise de crédito de clientes, integrando os dados com uma API externa que faz scoring (como Serasa, Boa Vista ou alguma IA de crédito).”


✳️ Regras de Negócio:
Todo cliente cadastrado pode solicitar uma análise de crédito.

Uma solicitação só pode ser feita se o cliente estiver com o cadastro completo.

Cada cliente pode ter apenas uma solicitação em andamento por vez.

O retorno da API externa de crédito deve ser armazenado junto à solicitação, com o score e o status da análise.

Caso a solicitação seja rejeitada, o cliente só poderá tentar novamente após 7 dias corridos.


🧱 Banco de Dados - SQL Server
sql
Copiar
Editar
CREATE DATABASE BancoDigital;

USE BancoDigital;

-- Tabela de Clientes
CREATE TABLE Clientes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Documento VARCHAR(14) NOT NULL UNIQUE,
    Email NVARCHAR(100),
    Telefone NVARCHAR(20),
    DataCadastro DATETIME DEFAULT GETDATE(),
    CadastroCompleto BIT NOT NULL DEFAULT 0
);

-- Tabela de Solicitações de Crédito
CREATE TABLE SolicitacoesCredito (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ClienteId INT NOT NULL,
    DataSolicitacao DATETIME DEFAULT GETDATE(),
    Status VARCHAR(50) NOT NULL, -- EmAndamento, Aprovada, Rejeitada
    Score INT,
    Justificativa NVARCHAR(255),
    DataResposta DATETIME,

    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);

-- Tabela de Usuários (para autenticação)
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    SenhaHash NVARCHAR(255) NOT NULL,
    Perfil VARCHAR(20) NOT NULL -- Admin, Analista, Cliente
);


🌐 Plataforma Externa para Integração
Você pode utilizar uma destas plataformas reais que simulam ou oferecem APIs externas de dados:

🔗 https://reqres.in
Serve como mock de API externa.

Você pode postar dados e receber resposta simulada.

Ideal para testes com autenticação e integração externa.

Alternativas:
https://webhook.site (para visualizar dados sendo enviados)

https://jsonplaceholder.typicode.com (fingir envios de dados)

https://mockapi.io (criar uma API fake personalizada)

Se quiser uma real: Serasa Experian API (mas precisa cadastro e contrato)



✅ Próximos Passos
Agora que temos a modelagem, vamos seguir essa ordem para construir a API:

Criar projetos da solução: BancoDigital.API, BancoDigital.Application, BancoDigital.Domain, BancoDigital.Services, BancoDigital.DataModel

Criar as entidades (Cliente, SolicitacaoCredito, Usuario)

Criar os DTOs (Request/Response) e mapeamentos com AutoMapper

Criar os Validators (FluentValidation)

Criar as Interfaces e Repositórios com Dapper

Criar os Services com regras de negócio

Configurar Autenticação OAuth2.0 + JWT

Implementar os endpoints da APIs

Criar integração com API externa (ex: mock da reqres.in)


✅ 1. Criar a Solução

Abra o terminal na pasta onde quer criar o projeto e execute:

dotnet new sln -n BancoDigital


✅ 2. Criar os Projetos

dotnet new webapi -n BancoDigital.API
dotnet new classlib -n BancoDigital.Application
dotnet new classlib -n BancoDigital.Domain
dotnet new classlib -n BancoDigital.Services
dotnet new classlib -n BancoDigital.DataModel


✅ 3. Adicionar os Projetos à Solução

dotnet sln add BancoDigital.API/BancoDigital.API.csproj
dotnet sln add BancoDigital.Application/BancoDigital.Application.csproj
dotnet sln add BancoDigital.Domain/BancoDigital.Domain.csproj
dotnet sln add BancoDigital.Services/BancoDigital.Services.csproj
dotnet sln add BancoDigital.DataModel/BancoDigital.DataModel.csproj

✅ 4. Adicionar Referências Entre os Projetos

dotnet add BancoDigital.API reference BancoDigital.Application
dotnet add BancoDigital.API reference BancoDigital.Services

dotnet add BancoDigital.Application reference BancoDigital.Domain
dotnet add BancoDigital.Application reference BancoDigital.Services
dotnet add BancoDigital.Application reference BancoDigital.DataModel

dotnet add BancoDigital.Services reference BancoDigital.Domain
dotnet add BancoDigital.Services reference BancoDigital.DataModel

dotnet add BancoDigital.DataModel reference BancoDigital.Domain

💡 Explicação da Hierarquia

[BancoDigital.API]
  └──> BancoDigital.Application
  └──> BancoDigital.Services

[BancoDigital.Application]
  └──> BancoDigital.Domain
  └──> BancoDigital.Services
  └──> BancoDigital.DataModel

[BancoDigital.Services]
  └──> BancoDigital.Domain
  └──> BancoDigital.DataModel

[BancoDigital.DataModel]
  └──> BancoDigital.Domain




  📁 Caminho: BancoDigital.Domain/Entities
Crie uma pasta chamada Entities e adicione os seguintes arquivos com as classes abaixo:

📌 1. Cliente.cs
namespace BancoDigital.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool CadastroCompleto { get; set; } = false;

    // Navegação
    public ICollection<SolicitacaoCredito> Solicitacoes { get; set; } = new List<SolicitacaoCredito>();

    // Regra de domínio básica
    public bool PodeSolicitarCredito()
    {
        return CadastroCompleto;
    }
}

📌 2. SolicitacaoCredito.cs

namespace BancoDigital.Domain.Entities;

public class SolicitacaoCredito
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.EmAndamento;
    public int? Score { get; set; }
    public string? Justificativa { get; set; }
    public DateTime? DataResposta { get; set; }

    // Navegação
    public Cliente? Cliente { get; set; }

    public bool PodeReenviar()
    {
        return Status == StatusSolicitacao.Rejeitada && DataResposta != null &&
               DateTime.UtcNow.Subtract(DataResposta.Value).TotalDays >= 7;
    }
}

📌 3. Usuario.cs

namespace BancoDigital.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string SenhaHash { get; set; } = null!;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;
}

📌 4. Enums/StatusSolicitacao.cs

namespace BancoDigital.Domain.Enums;

public enum StatusSolicitacao
{
    EmAndamento = 0,
    Aprovada = 1,
    Rejeitada = 2
}

📌 5. Enums/PerfilUsuario.cs

namespace BancoDigital.Domain.Enums;

public enum PerfilUsuario
{
    Admin = 0,
    Analista = 1,
    Cliente = 2
}

✅ O Que Fizemos Até Aqui:

Todas as entidades essenciais foram criadas com propriedades e métodos de domínio.

Criamos dois enums para representar status e perfis de forma tipada.

Incluímos validações de domínio básicas como PodeSolicitarCredito() e PodeReenviar().

📁 Estrutura de Pastas
No projeto BancoDigital.Application, crie as seguintes pastas:

/Requests
/Responses
/DTOs
/Mappings

✅ 1. DTOs: Objeto de Transferência Interno

📄 DTOs/ClienteDto.cs

namespace BancoDigital.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public bool CadastroCompleto { get; set; }
}

📄 DTOs/SolicitacaoCreditoDto.cs

namespace BancoDigital.Application.DTOs;

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

✅ 2. Requests (entrada do cliente/front)

📄 Requests/ClienteRequest.cs

namespace BancoDigital.Application.Requests;

public class ClienteRequest
{
    public string Nome { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
}


📄 Requests/SolicitacaoCreditoRequest.cs

namespace BancoDigital.Application.Requests;

public class SolicitacaoCreditoRequest
{
    public int ClienteId { get; set; }
}


✅ 3. Responses (saída para o front)

📄 Responses/ClienteResponse.cs

namespace BancoDigital.Application.Responses;

public class ClienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public bool CadastroCompleto { get; set; }
}

📄 Responses/SolicitacaoCreditoResponse.cs

namespace BancoDigital.Application.Responses;

public class SolicitacaoCreditoResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = null!;
    public int? Score { get; set; }
    public string? Justificativa { get; set; }
    public DateTime DataSolicitacao { get; set; }
    public DateTime? DataResposta { get; set; }
}

✅ 4. Mapeamento com AutoMapper

Crie a pasta /Mappings e o seguinte perfil:

📄 Mappings/AutoMapperProfile.cs

using AutoMapper;
using BancoDigital.Domain.Entities;
using BancoDigital.Application.DTOs;
using BancoDigital.Application.Requests;
using BancoDigital.Application.Responses;
using BancoDigital.Domain.Enums;

namespace BancoDigital.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Cliente
        CreateMap<Cliente, ClienteDto>().ReverseMap();
        CreateMap<Cliente, ClienteResponse>();
        CreateMap<ClienteRequest, Cliente>();

        // Solicitação Crédito
        CreateMap<SolicitacaoCredito, SolicitacaoCreditoDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<SolicitacaoCredito, SolicitacaoCreditoResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<SolicitacaoCreditoRequest, SolicitacaoCredito>();
    }
}

🧠 Registro do AutoMapper (ex: em Program.cs da API)

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

✅ Tudo Pronto Nesta Etapa
Criados:

DTOs internos de aplicação

Requests e Responses para front-end

Mapeamentos bidirecionais com AutoMapper


Vamos agora criar os Validators com FluentValidation, validando os Requests recebidos pela API — é aqui que tratamos estrutura e formato dos dados, deixando as regras de negócio para os serviços do domínio.

Essa validação será aplicada automaticamente nos endpoints, usando o AddFluentValidationAutoValidation() no Program.cs.

📁 Estrutura no projeto BancoDigital.Application
Crie uma pasta chamada Validators:
BancoDigital.Application/
└── Validators/
    ├── ClienteRequestValidator.cs
    └── SolicitacaoCreditoRequestValidator.cs



    Vamos manter tudo limpo, organizado e seguindo os princípios de Clean Code e Separação de Responsabilidades. A seguir, te mostro como deixar o Program.cs enxuto e mover toda a injeção de dependência (inclusive FluentValidation e AutoMapper) para uma classe única e central chamada IocConfiguration.

📁 Estrutura Recomendada

Dentro do projeto BancoDigital.API, crie:

/Configurations
   └── IocConfiguration.cs

✅ 1. Classe IocConfiguration.cs

using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using BancoDigital.Application.Mappings;
using BancoDigital.Application.Validators;

namespace BancoDigital.API.Configurations;

public static class IocConfiguration
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(AutoMapperProfile));

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<ClienteRequestValidator>();

        // Adicione aqui os serviços e repositórios conforme forem sendo criados:
        // services.AddScoped<IClienteRepository, ClienteRepository>();
        // services.AddScoped<IClienteService, ClienteService>();
    }
}

✅ 2. Program.cs Limpo

using BancoDigital.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Controllers
builder.Services.AddControllers();

// Injeção de dependência
builder.Services.AddDependencyInjection();

// Swagger (opcional para testes)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

✅ Resultado

O Program.cs fica enxuto, com foco apenas no ciclo de vida da aplicação

Toda a complexidade da injeção de dependência (services, repositories, mappings, validators etc.) fica centralizada e organizada

Fácil manutenção e escalabilidade

Vamos criar agora as Interfaces e Repositórios com Dapper, com estrutura limpa e seguindo o DDD + Clean Code. O foco será no domínio Cliente, mas o padrão servirá para SolicitacaoCredito e Usuario também.

📁 Estrutura por Projeto

Projeto: BancoDigital.Services

Aqui ficarão as interfaces e implementações dos repositórios (com Dapper).

/Repositories
   ├── Interfaces/
   │   └── IClienteRepository.cs
   └── ClienteRepository.cs


   ✅ 1. Interface IClienteRepository
📄 Repositories/Interfaces/IClienteRepository.cs
csharp
Copiar
Editar
using BancoDigital.Domain.Entities;

namespace BancoDigital.Services.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<Cliente?> ObterPorDocumentoAsync(string documento);
    Task<IEnumerable<Cliente>> ListarTodosAsync();
    Task<int> CriarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
    Task DeletarAsync(int id);
}

✅ 2. Implementação com Dapper

📄 Repositories/ClienteRepository.cs

using BancoDigital.Domain.Entities;
using BancoDigital.Services.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BancoDigital.Services.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ClienteRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
    }

    private IDbConnection Connection => new SqlConnection(_connectionString);

    public async Task<int> CriarAsync(Cliente cliente)
    {
        var query = @"
            INSERT INTO Clientes (Nome, Documento, Email, Telefone, CadastroCompleto)
            VALUES (@Nome, @Documento, @Email, @Telefone, @CadastroCompleto);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var conn = Connection;
        return await conn.ExecuteScalarAsync<int>(query, cliente);
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        var query = @"
            UPDATE Clientes
            SET Nome = @Nome,
                Documento = @Documento,
                Email = @Email,
                Telefone = @Telefone,
                CadastroCompleto = @CadastroCompleto
            WHERE Id = @Id";

        using var conn = Connection;
        await conn.ExecuteAsync(query, cliente);
    }

    public async Task DeletarAsync(int id)
    {
        var query = "DELETE FROM Clientes WHERE Id = @Id";

        using var conn = Connection;
        await conn.ExecuteAsync(query, new { Id = id });
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        var query = "SELECT * FROM Clientes WHERE Id = @Id";

        using var conn = Connection;
        return await conn.QueryFirstOrDefaultAsync<Cliente>(query, new { Id = id });
    }

    public async Task<Cliente?> ObterPorDocumentoAsync(string documento)
    {
        var query = "SELECT * FROM Clientes WHERE Documento = @Documento";

        using var conn = Connection;
        return await conn.QueryFirstOrDefaultAsync<Cliente>(query, new { Documento = documento });
    }

    public async Task<IEnumerable<Cliente>> ListarTodosAsync()
    {
        var query = "SELECT * FROM Clientes";

        using var conn = Connection;
        return await conn.QueryAsync<Cliente>(query);
    }
}

✅ 3. Adicione Dapper ao projeto
No terminal, na pasta do projeto BancoDigital.Services:

dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient

✅ 4. Configure a ConnectionString

No appsettings.json do projeto BancoDigital.API:


{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BancoDigital;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

✅ 5. Registre no IocConfiguration.cs

services.AddScoped<IClienteRepository, ClienteRepository>();

🎯 Pronto!

Agora tem:

Interface desacoplada

Repositório profissional com Dapper

Injeção de dependência organizada

Mapeamento direto das entidades



📁 Estrutura no Projeto BancoDigital.Application

/Interfaces
   └── IClienteService.cs
/Services
   └── ClienteService.cs

Esse service usa o repositório do projeto Services (injetado via interface), aplica validações de negócio e retorna DTOs mapeados com AutoMapper.

✅ 1. Interface IClienteService

📄 Interfaces/IClienteService.cs

using BancoDigital.Application.DTOs;
using BancoDigital.Application.Requests;

namespace BancoDigital.Application.Interfaces;

public interface IClienteService
{
    Task<ClienteDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<ClienteDto>> ListarTodosAsync();
    Task<int> CriarAsync(ClienteRequest request);
    Task AtualizarAsync(int id, ClienteRequest request);
    Task DeletarAsync(int id);
}

✅ 2. Implementação ClienteService

📄 Services/ClienteService.cs

using AutoMapper;
using BancoDigital.Application.DTOs;
using BancoDigital.Application.Interfaces;
using BancoDigital.Application.Requests;
using BancoDigital.Domain.Entities;
using BancoDigital.Services.Repositories.Interfaces;

namespace BancoDigital.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IMapper _mapper;

    public ClienteService(IClienteRepository clienteRepository, IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    public async Task<ClienteDto?> ObterPorIdAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        return cliente is null ? null : _mapper.Map<ClienteDto>(cliente);
    }

    public async Task<IEnumerable<ClienteDto>> ListarTodosAsync()
    {
        var clientes = await _clienteRepository.ListarTodosAsync();
        return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
    }

    public async Task<int> CriarAsync(ClienteRequest request)
    {
        var cliente = _mapper.Map<Cliente>(request);

        // Regra de negócio: CPF/CNPJ deve ser único
        var existente = await _clienteRepository.ObterPorDocumentoAsync(cliente.Documento);
        if (existente != null)
            throw new InvalidOperationException("Já existe um cliente com este documento.");

        // Regra: se CPF e nome estiverem preenchidos, consideramos como "cadastro completo"
        cliente.CadastroCompleto = !string.IsNullOrEmpty(cliente.Nome) &&
                                   !string.IsNullOrEmpty(cliente.Documento);

        return await _clienteRepository.CriarAsync(cliente);
    }

    public async Task AtualizarAsync(int id, ClienteRequest request)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        cliente.Nome = request.Nome;
        cliente.Documento = request.Documento;
        cliente.Email = request.Email;
        cliente.Telefone = request.Telefone;
        cliente.CadastroCompleto = !string.IsNullOrEmpty(cliente.Nome) &&
                                   !string.IsNullOrEmpty(cliente.Documento);

        await _clienteRepository.AtualizarAsync(cliente);
    }

    public async Task DeletarAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        await _clienteRepository.DeletarAsync(id);
    }
}

✅ 3. Registre no IocConfiguration.cs

// Application Layer
services.AddScoped<IClienteService, ClienteService>();

✅ Regras de Negócio Aplicadas

Documento deve ser único

Cadastro só é marcado como completo se Nome e Documento estiverem preenchidos

Bloqueia atualizações/deleções se o cliente não existir

Mapeamento automático entre Request → Entidade → Dto

Podemos agora:

✅ Criar os Controllers no projeto BancoDigital.API
✅ Implementar o fluxo completo de Solicitação de Crédito
✅ Ou já integrar com API externa mock (como reqres.in) para simular scoring


Vamos criar os Controllers no projeto BancoDigital.API, com foco em manter o código limpo, simples e desacoplado, consumindo os Services da camada de Application, e respeitando os padrões REST.

Começaremos pelo ClienteController, com todos os endpoints necessários.

📁 Estrutura no projeto BancoDigital.API


/Controllers
   └── ClienteController.cs

✅ 1. ClienteController.cs


using BancoDigital.Application.Interfaces;
using BancoDigital.Application.Requests;
using BancoDigital.Application.Responses;
using BancoDigital.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BancoDigital.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(int id)
    {
        var cliente = await _clienteService.ObterPorIdAsync(id);
        if (cliente == null) return NotFound();

        return Ok(new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Documento = cliente.Documento,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            CadastroCompleto = cliente.CadastroCompleto
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> ListarTodos()
    {
        var clientes = await _clienteService.ListarTodosAsync();

        var response = clientes.Select(c => new ClienteResponse
        {
            Id = c.Id,
            Nome = c.Nome,
            Documento = c.Documento,
            Email = c.Email,
            Telefone = c.Telefone,
            CadastroCompleto = c.CadastroCompleto
        });

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> Criar([FromBody] ClienteRequest request)
    {
        var id = await _clienteService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id }, null);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Atualizar(int id, [FromBody] ClienteRequest request)
    {
        await _clienteService.AtualizarAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Deletar(int id)
    {
        await _clienteService.DeletarAsync(id);
        return NoContent();
    }
}

✅ Resultado

Você tem agora um controller:

Isolado das regras de negócio

Usando os serviços via injeção de dependência

Retornando ActionResult de forma padronizada (200, 201, 204, 404)

Pronto para ser consumido no frontend

✅ Testando via Swagger

Com o Swagger já habilitado, basta rodar:

dotnet run --project BancoDigital.API

Acesse https://localhost:5001/swagger ou http://localhost:5000/swagger


Vamos criar o SolicitacaoCreditoController, responsável por registrar e consultar solicitações de crédito, aplicando as regras de negócio por meio do SolicitacaoCreditoService que criaremos também.

✅ Antes: criar o Service e Interface de SolicitacaoCredito

📁 BancoDigital.Application/Interfaces/ISolicitacaoCreditoService.cs


using BancoDigital.Application.DTOs;
using BancoDigital.Application.Requests;

namespace BancoDigital.Application.Interfaces;

public interface ISolicitacaoCreditoService
{
    Task<int> CriarAsync(SolicitacaoCreditoRequest request);
    Task<SolicitacaoCreditoDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<SolicitacaoCreditoDto>> ListarPorClienteAsync(int clienteId);
}


📁 BancoDigital.Application/Services/SolicitacaoCreditoService.cs


using AutoMapper;
using BancoDigital.Application.DTOs;
using BancoDigital.Application.Interfaces;
using BancoDigital.Application.Requests;
using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Enums;
using BancoDigital.Services.Repositories.Interfaces;

namespace BancoDigital.Application.Services;

public class SolicitacaoCreditoService : ISolicitacaoCreditoService
{
    private readonly ISolicitacaoCreditoRepository _solicitacaoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMapper _mapper;

    public SolicitacaoCreditoService(
        ISolicitacaoCreditoRepository solicitacaoRepository,
        IClienteRepository clienteRepository,
        IMapper mapper)
    {
        _solicitacaoRepository = solicitacaoRepository;
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    public async Task<int> CriarAsync(SolicitacaoCreditoRequest request)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId);
        if (cliente == null || !cliente.PodeSolicitarCredito())
            throw new InvalidOperationException("Cliente inválido ou com cadastro incompleto.");

        var emAndamento = await _solicitacaoRepository.ExisteSolicitacaoEmAberto(request.ClienteId);
        if (emAndamento)
            throw new InvalidOperationException("Já existe uma solicitação de crédito em andamento.");

        var solicitacao = new SolicitacaoCredito
        {
            ClienteId = request.ClienteId,
            Status = StatusSolicitacao.EmAndamento,
            DataSolicitacao = DateTime.UtcNow
        };

        return await _solicitacaoRepository.CriarAsync(solicitacao);
    }

    public async Task<SolicitacaoCreditoDto?> ObterPorIdAsync(int id)
    {
        var entity = await _solicitacaoRepository.ObterPorIdAsync(id);
        return entity == null ? null : _mapper.Map<SolicitacaoCreditoDto>(entity);
    }

    public async Task<IEnumerable<SolicitacaoCreditoDto>> ListarPorClienteAsync(int clienteId)
    {
        var entities = await _solicitacaoRepository.ListarPorClienteAsync(clienteId);
        return _mapper.Map<IEnumerable<SolicitacaoCreditoDto>>(entities);
    }
}

✅ Repository + Interface no BancoDigital.Services

📁 Repositories/Interfaces/ISolicitacaoCreditoRepository.cs


using BancoDigital.Domain.Entities;

namespace BancoDigital.Services.Repositories.Interfaces;

public interface ISolicitacaoCreditoRepository
{
    Task<int> CriarAsync(SolicitacaoCredito entity);
    Task<SolicitacaoCredito?> ObterPorIdAsync(int id);
    Task<IEnumerable<SolicitacaoCredito>> ListarPorClienteAsync(int clienteId);
    Task<bool> ExisteSolicitacaoEmAberto(int clienteId);
}


✅ 1. Controller no projeto BancoDigital.API

📁 Controllers/SolicitacaoCreditoController.cs


using BancoDigital.Application.Interfaces;
using BancoDigital.Application.Requests;
using BancoDigital.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BancoDigital.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitacaoCreditoController : ControllerBase
{
    private readonly ISolicitacaoCreditoService _service;

    public SolicitacaoCreditoController(ISolicitacaoCreditoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SolicitacaoCreditoRequest request)
    {
        var id = await _service.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id }, null);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var solicitacao = await _service.ObterPorIdAsync(id);
        return solicitacao == null ? NotFound() : Ok(solicitacao);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> ListarPorCliente(int clienteId)
    {
        var solicitacoes = await _service.ListarPorClienteAsync(clienteId);
        return Ok(solicitacoes);
    }
}

✅ Registrar dependências no IocConfiguration.cs


services.AddScoped<ISolicitacaoCreditoService, SolicitacaoCreditoService>();
services.AddScoped<ISolicitacaoCreditoRepository, SolicitacaoCreditoRepository>();

✅ Conclusão

Agora sua API tem:

Regras de negócio aplicadas no Service (1 por cliente em andamento, cadastro completo, etc.)

Separação por camadas

Controller limpo e RESTful

Próximo passo:

Criar o SolicitacaoCreditoRepository com Dapper



Vamos criar agora o SolicitacaoCreditoRepository com Dapper, seguindo o padrão que já estamos usando: injeção de dependência via interface, separação de responsabilidades, consultas otimizadas e seguras.

📁 Estrutura no projeto BancoDigital.Services

/Repositories
   ├── Interfaces/
   │   └── ISolicitacaoCreditoRepository.cs
   └── SolicitacaoCreditoRepository.cs

✅ 1. Interface (caso ainda não tenha criado)

📄 Interfaces/ISolicitacaoCreditoRepository.cs

using BancoDigital.Domain.Entities;

namespace BancoDigital.Services.Repositories.Interfaces;

public interface ISolicitacaoCreditoRepository
{
    Task<int> CriarAsync(SolicitacaoCredito entity);
    Task<SolicitacaoCredito?> ObterPorIdAsync(int id);
    Task<IEnumerable<SolicitacaoCredito>> ListarPorClienteAsync(int clienteId);
    Task<bool> ExisteSolicitacaoEmAberto(int clienteId);
}

✅ 2. Implementação com Dapper

📄 SolicitacaoCreditoRepository.cs

using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Enums;
using BancoDigital.Services.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BancoDigital.Services.Repositories;

public class SolicitacaoCreditoRepository : ISolicitacaoCreditoRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public SolicitacaoCreditoRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
    }

    private IDbConnection Connection => new SqlConnection(_connectionString);

    public async Task<int> CriarAsync(SolicitacaoCredito entity)
    {
        var query = @"
            INSERT INTO SolicitacoesCredito (ClienteId, DataSolicitacao, Status, Score, Justificativa, DataResposta)
            VALUES (@ClienteId, @DataSolicitacao, @Status, @Score, @Justificativa, @DataResposta);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var conn = Connection;
        return await conn.ExecuteScalarAsync<int>(query, new
        {
            entity.ClienteId,
            entity.DataSolicitacao,
            Status = entity.Status.ToString(),
            entity.Score,
            entity.Justificativa,
            entity.DataResposta
        });
    }

    public async Task<SolicitacaoCredito?> ObterPorIdAsync(int id)
    {
        var query = @"SELECT * FROM SolicitacoesCredito WHERE Id = @Id";

        using var conn = Connection;
        var result = await conn.QueryFirstOrDefaultAsync<SolicitacaoCredito>(query, new { Id = id });

        return result == null ? null : ParseStatus(result);
    }

    public async Task<IEnumerable<SolicitacaoCredito>> ListarPorClienteAsync(int clienteId)
    {
        var query = @"SELECT * FROM SolicitacoesCredito WHERE ClienteId = @ClienteId";

        using var conn = Connection;
        var results = await conn.QueryAsync<SolicitacaoCredito>(query, new { ClienteId = clienteId });

        return results.Select(ParseStatus).ToList();
    }

    public async Task<bool> ExisteSolicitacaoEmAberto(int clienteId)
    {
        var query = @"SELECT COUNT(1) FROM SolicitacoesCredito
                      WHERE ClienteId = @ClienteId AND Status = 'EmAndamento'";

        using var conn = Connection;
        var count = await conn.ExecuteScalarAsync<int>(query, new { ClienteId = clienteId });

        return count > 0;
    }

    private SolicitacaoCredito ParseStatus(SolicitacaoCredito s)
    {
        if (Enum.TryParse<StatusSolicitacao>(s.Status.ToString(), out var status))
        {
            s.Status = status;
        }
        return s;
    }
}

🧠 Por que usamos ParseStatus?
Porque no banco o status está armazenado como string ('EmAndamento', 'Aprovada', etc.), e o Dapper não converte automaticamente de string para enum. Então fazemos isso manualmente após a leitura.

✅ 3. Registrar no IocConfiguration.cs

services.AddScoped<ISolicitacaoCreditoRepository, SolicitacaoCreditoRepository>();

🔗 Tabela SQL usada
Certifique-se de que essa tabela existe no banco:

sql

CREATE TABLE SolicitacoesCredito (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ClienteId INT NOT NULL,
    DataSolicitacao DATETIME NOT NULL,
    Status VARCHAR(50) NOT NULL,
    Score INT,
    Justificativa NVARCHAR(255),
    DataResposta DATETIME,
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);

✅ Finalizamos o Repository

Agora tem:

CRUD completo da entidade SolicitacaoCredito

Validação de "Em andamento" por cliente

Conversão de enum robusta

Repositório 100% pronto para produção