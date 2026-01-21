using FluentValidation;

namespace EDA.APPLICATION.Features.ProductFeature.Commands.CreateProductCommand
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del producto no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre del producto no puede exceder los 100 caracteres.");

            RuleFor(x => x.Barcode)
                .MaximumLength(50).WithMessage("El código de barras no puede exceder los 50 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Barcode));

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a 0.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("El stock debe ser mayor o igual a 0.");

            RuleFor(x => x.FamilyId)
                .GreaterThan(0).WithMessage("Debe seleccionar una familia.");

            RuleFor(x => x.TaxId)
                .GreaterThan(0).WithMessage("Debe seleccionar un impuesto.");
        }
    }
}
