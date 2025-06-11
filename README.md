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

