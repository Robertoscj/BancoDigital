using BancoDigital.Application.Mappings;
using BancoDigital.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace BancoDigital.API.IocConfigurations
{
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
}