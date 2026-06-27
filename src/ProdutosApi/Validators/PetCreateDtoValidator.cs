using FluentValidation;
using ProdutosApi.Dtos;

namespace ProdutosApi.Validators;

public class PetCreateDtoValidator : AbstractValidator<PetCreateDto>
{
    public PetCreateDtoValidator()
    {
        RuleFor(p => p.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(80).WithMessage("O nome deve ter no máximo 80 caracteres.");

        RuleFor(p => p.Raça)
            .NotEmpty().WithMessage("A raça é obrigatória.")
            .MaximumLength(80).WithMessage("A raça deve ter no máximo 80 caracteres.");

        RuleFor(p => p.Cor)
            .NotEmpty().WithMessage("A cor é obrigatória.")
            .MaximumLength(50).WithMessage("A cor deve ter no máximo 50 caracteres.");
    }
}
