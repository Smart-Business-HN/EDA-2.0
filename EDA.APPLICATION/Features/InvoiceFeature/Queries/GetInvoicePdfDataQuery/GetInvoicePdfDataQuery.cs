using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoicePaymentSpecification;
using EDA.APPLICATION.Specifications.SoldProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Queries.GetInvoicePdfDataQuery
{
    public class GetInvoicePdfDataQuery : IRequest<Result<InvoicePdfData>>
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public Cai Cai { get; set; } = null!;
    }

    public class GetInvoicePdfDataQueryHandler : IRequestHandler<GetInvoicePdfDataQuery, Result<InvoicePdfData>>
    {
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly IRepositoryAsync<SoldProduct> _soldProductRepository;
        private readonly IRepositoryAsync<InvoicePayment> _paymentRepository;
        private readonly IRepositoryAsync<Tax> _taxRepository;

        public GetInvoicePdfDataQueryHandler(
            IRepositoryAsync<Company> companyRepository,
            IRepositoryAsync<SoldProduct> soldProductRepository,
            IRepositoryAsync<InvoicePayment> paymentRepository,
            IRepositoryAsync<Tax> taxRepository)
        {
            _companyRepository = companyRepository;
            _soldProductRepository = soldProductRepository;
            _paymentRepository = paymentRepository;
            _taxRepository = taxRepository;
        }

        public async Task<Result<InvoicePdfData>> Handle(GetInvoicePdfDataQuery request, CancellationToken cancellationToken)
        {
            // Obtener empresa
            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            if (company == null)
            {
                company = new Company { Name = "Mi Empresa", Owner = "Propietario" };
            }

            // Obtener productos vendidos
            var soldProducts = await _soldProductRepository.ListAsync(
                new GetSoldProductsByInvoiceIdSpecification(request.InvoiceId),
                cancellationToken);

            // Obtener pagos
            var payments = await _paymentRepository.ListAsync(
                new GetPaymentsByInvoiceIdsSpecification(new List<int> { request.InvoiceId }),
                cancellationToken);

            // Obtener impuestos para lookup
            var taxes = await _taxRepository.ListAsync(cancellationToken);

            // Construir datos del PDF
            var pdfData = new InvoicePdfData
            {
                // Datos de la empresa
                CompanyName = company.Name,
                CompanyOwner = company.Owner,
                CompanyRtn = company.RTN,
                CompanyAddress1 = company.Address1,
                CompanyAddress2 = company.Address2,
                CompanyPhone = company.PhoneNumber1,
                CompanyEmail = company.Email,
                CompanyLogo = company.Logo,

                // Datos de la factura
                InvoiceNumber = request.Invoice.InvoiceNumber,
                Date = request.Invoice.Date,
                Subtotal = request.Invoice.Subtotal,
                TotalDiscounts = request.Invoice.TotalDiscounts,
                TotalTaxes = request.Invoice.TotalTaxes,
                Total = request.Invoice.Total,
                TaxedAt15Percent = request.Invoice.TaxedAt15Percent,
                TaxesAt15Percent = request.Invoice.TaxesAt15Percent,
                TaxedAt18Percent = request.Invoice.TaxedAt18Percent,
                TaxesAt18Percent = request.Invoice.TaxesAt18Percent,
                Exempt = request.Invoice.Exempt,
                CashReceived = request.Invoice.CashReceived,
                ChangeGiven = request.Invoice.ChangeGiven,

                // Datos del cliente
                CustomerName = request.Customer.Name,
                CustomerRtn = request.Customer.RTN,

                // Datos del CAI
                CaiNumber = request.Cai.Code,
                CaiFromDate = request.Cai.FromDate,
                CaiToDate = request.Cai.ToDate,
                InitialCorrelative = $"{request.Cai.Prefix}{request.Cai.InitialCorrelative:D8}",
                FinalCorrelative = $"{request.Cai.Prefix}{request.Cai.FinalCorrelative:D8}",

                // Items
                Items = soldProducts.Select(sp => new InvoicePdfItem
                {
                    Description = sp.Description ?? "Producto",
                    Quantity = sp.Quantity,
                    UnitPrice = sp.UnitPrice,
                    TaxPercentage = taxes.FirstOrDefault(t => t.Id == sp.TaxId)?.Percentage ?? 0m,
                    TotalLine = sp.TotalLine
                }).ToList(),

                // Pagos
                Payments = payments.Select(ip => new InvoicePdfPayment
                {
                    PaymentTypeName = ip.PaymentType?.Name ?? "Pago",
                    Amount = ip.Amount
                }).ToList()
            };

            return new Result<InvoicePdfData>(pdfData);
        }
    }
}
