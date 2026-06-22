using FluentValidation;
using ProdutosApi.Dtos;

namespace ProdutosApi.Validators;

public class CategoriaUpdateDtoValidator : AbstractValidator<CategoriaUpdateDto>
{
    public CategoriaUpdateDtoValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(80).WithMessage("O nome deve ter no máximo 80 caracteres.");

        RuleFor(c => c.Descricao)
            .MaximumLength(300).WithMessage("A descrição deve ter no máximo 300 caracteres.");
    }
}
