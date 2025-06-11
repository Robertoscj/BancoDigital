using BancoDigital.Application.Requests;
using FluentValidation;

namespace BancoDigital.Application.Validators
{
   public class SolicitacaoCreditoRequestValidator : AbstractValidator<SolicitacaoCreditoRequest>
{
    public SolicitacaoCreditoRequestValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0).WithMessage("O ID do cliente deve ser maior que zero.");
    }
}
}