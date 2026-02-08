using FluentValidation;
using System.Text.RegularExpressions;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Commands.UpdatePurchaseBillCommand
{
    public class UpdatePurchaseBillCommandValidator : AbstractValidator<UpdatePurchaseBillCommand>
    {
        private static readonly Regex CaiRegex = new Regex(
            @"^[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{2}$",
            RegexOptions.Compiled);

        public UpdatePurchaseBillCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID de la factura es invalido.");

            RuleFor(x => x.ProviderId)
                .GreaterThan(0).WithMessage("Debe seleccionar un proveedor.");

            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("El numero de factura es requerido.")
                .MaximumLength(50).WithMessage("El numero de factura no puede exceder 50 caracteres.");

            RuleFor(x => x.InvoiceDate)
                .NotEmpty().WithMessage("La fecha de factura es requerida.");

            RuleFor(x => x.Cai)
                .NotEmpty().WithMessage("El CAI es requerido.")
                .MaximumLength(100).WithMessage("El CAI no puede exceder 100 caracteres.")
                .Must(cai => CaiRegex.IsMatch(cai ?? ""))
                .WithMessage("El CAI debe tener el formato XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX (hexadecimal).");

            RuleFor(x => x.Total)
                .GreaterThan(0).WithMessage("El total debe ser mayor a cero.");

            RuleFor(x => x.ExpenseAccountId)
                .GreaterThan(0).WithMessage("Debe seleccionar una cuenta de gastos.");

            RuleFor(x => x.Exempt).GreaterThanOrEqualTo(0).WithMessage("El monto exento no puede ser negativo.");
            RuleFor(x => x.Exonerated).GreaterThanOrEqualTo(0).WithMessage("El monto exonerado no puede ser negativo.");
            RuleFor(x => x.TaxedAt15Percent).GreaterThanOrEqualTo(0).WithMessage("El monto gravado 15% no puede ser negativo.");
            RuleFor(x => x.TaxedAt18Percent).GreaterThanOrEqualTo(0).WithMessage("El monto gravado 18% no puede ser negativo.");
            RuleFor(x => x.Taxes15Percent).GreaterThanOrEqualTo(0).WithMessage("El impuesto 15% no puede ser negativo.");
            RuleFor(x => x.Taxes18Percent).GreaterThanOrEqualTo(0).WithMessage("El impuesto 18% no puede ser negativo.");
        }
    }
}
