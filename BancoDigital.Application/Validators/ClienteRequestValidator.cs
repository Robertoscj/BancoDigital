using BancoDigital.Application.Requests;
using BancoDigital.Domain.Entities;
using FluentValidation;

namespace BancoDigital.Application.Validators
{
    public class ClienteRequestValidator : AbstractValidator<ClienteRequest>
    {
        public ClienteRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.");

            RuleFor(x => x.Documento)
                .NotEmpty().WithMessage("O CPF/CNPJ é obrigatório.")
                .Length(11, 14).WithMessage("O CPF deve ter 11 ou o CNPJ 14 caracteres.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("O e-mail informado não é válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Telefone)
                .Matches(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$")
                .WithMessage("O telefone informado não é válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Telefone));
        }
    }
}