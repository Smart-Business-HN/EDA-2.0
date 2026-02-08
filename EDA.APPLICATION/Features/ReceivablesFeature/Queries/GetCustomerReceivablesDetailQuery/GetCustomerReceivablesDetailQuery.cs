using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ReceivablesSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReceivablesFeature.Queries.GetCustomerReceivablesDetailQuery
{
    public class GetCustomerReceivablesDetailQuery : IRequest<Result<CustomerReceivablesDetail>>
    {
        public int CustomerId { get; set; }
    }

    public class GetCustomerReceivablesDetailQueryHandler
        : IRequestHandler<GetCustomerReceivablesDetailQuery, Result<CustomerReceivablesDetail>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<Customer> _customerRepository;

        public GetCustomerReceivablesDetailQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<Customer> customerRepository)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Result<CustomerReceivablesDetail>> Handle(
            GetCustomerReceivablesDetailQuery request,
            CancellationToken cancellationToken)
        {
            // Obtener datos del cliente
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                return new Result<CustomerReceivablesDetail>("Cliente no encontrado");
            }

            // Obtener facturas pendientes del cliente
            var invoices = await _invoiceRepository.ListAsync(
                new GetPendingInvoicesByCustomerSpecification(request.CustomerId),
                cancellationToken);

            var today = DateTime.Today;
            var in7Days = today.AddDays(7);

            var pendingInvoices = invoices.Select(inv =>
            {
                var daysOverdue = inv.DueDate.HasValue
                    ? (today - inv.DueDate.Value.Date).Days
                    : 0;

                return new PendingInvoiceItem
                {
                    InvoiceId = inv.Id,
                    InvoiceNumber = inv.InvoiceNumber,
                    IssueDate = inv.Date,
                    DueDate = inv.DueDate,
                    Total = inv.Total,
                    OutstandingAmount = inv.OutstandingAmount,
                    DaysOverdue = daysOverdue,
                    IsOverdue = daysOverdue > 0
                };
            }).ToList();

            var detail = new CustomerReceivablesDetail
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerRtn = customer.RTN,
                CustomerCompany = customer.Company,
                CustomerEmail = customer.Email,
                CustomerPhone = customer.PhoneNumber,
                TotalOwed = invoices.Sum(i => i.OutstandingAmount),
                PendingInvoicesCount = invoices.Count,
                OverdueAmount = invoices
                    .Where(i => i.DueDate.HasValue && i.DueDate.Value.Date < today)
                    .Sum(i => i.OutstandingAmount),
                DueIn7DaysAmount = invoices
                    .Where(i => i.DueDate.HasValue &&
                               i.DueDate.Value.Date >= today &&
                               i.DueDate.Value.Date <= in7Days)
                    .Sum(i => i.OutstandingAmount),
                PendingInvoices = pendingInvoices
            };

            return new Result<CustomerReceivablesDetail>(detail);
        }
    }
}
