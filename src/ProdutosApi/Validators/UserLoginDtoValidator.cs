using FluentValidation;
using ProdutosApi.Dtos;

namespace ProdutosApi.Validators;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.");

        RuleFor(u => u.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
