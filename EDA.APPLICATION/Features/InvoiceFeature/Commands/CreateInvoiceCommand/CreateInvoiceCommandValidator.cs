using FluentValidation;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.CreateInvoiceCommand
{
    public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
    {
        public CreateInvoiceCommandValidator()
        {
            // Debe tener productos
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("La factura debe contener al menos un producto.")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("La factura debe contener al menos un producto.");

            // Debe tener cliente
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Debe seleccionar un cliente.");

            // Debe tener CAI
            RuleFor(x => x.CaiId)
                .GreaterThan(0).WithMessage("Debe tener un CAI activo para facturar.");

            // Debe tener fecha
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("La fecha de la factura es requerida.");

            // Debe tener usuario
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Debe tener un usuario identificado para facturar.");

            // Validar totales
            RuleFor(x => x.Total)
                .GreaterThan(0).WithMessage("El total de la factura debe ser mayor a 0.");

            // Facturas de contado: debe tener pagos que cubran el total
            When(x => !x.IsCredit, () =>
            {
                RuleFor(x => x.Payments)
                    .NotEmpty().WithMessage("Debe agregar al menos un método de pago.");

                RuleFor(x => x)
                    .Must(cmd => cmd.Payments.Sum(p => p.Amount) >= cmd.Total)
                    .WithMessage("El total de los pagos debe cubrir el total de la factura.");
            });

            // Facturas al credito: debe tener dias de credito y cliente identificado
            When(x => x.IsCredit, () =>
            {
                RuleFor(x => x.CreditDays)
                    .NotNull().WithMessage("Debe especificar los dias de credito.")
                    .GreaterThan(0).WithMessage("Los dias de credito deben ser mayor a 0.");

                RuleFor(x => x.CustomerId)
                    .NotEqual(1).WithMessage("Las facturas al credito requieren un cliente identificado (no Consumidor Final).");
            });

            // Validar cada item
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("ID de producto inválido.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

                item.RuleFor(i => i.TaxId)
                    .GreaterThan(0).WithMessage("Debe especificar el impuesto del producto.");

                item.RuleFor(i => i.UnitPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("El precio unitario debe ser mayor o igual a 0.");
            });

            // Validar cada pago (cuando hay pagos)
            RuleForEach(x => x.Payments).ChildRules(payment =>
            {
                payment.RuleFor(p => p.PaymentTypeId)
                    .GreaterThan(0).WithMessage("Tipo de pago inválido.");

                payment.RuleFor(p => p.Amount)
                    .GreaterThan(0).WithMessage("El monto del pago debe ser mayor a 0.");
            });
        }
    }
}
