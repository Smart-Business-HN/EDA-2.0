using System;
using System.Collections.Generic;
using System.Text;

namespace EDA.DOMAIN.Entities
{
    public class Cai
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int InitialCorrelative { get; set; }
        public int FinalCorrelative { get; set; }
        public int CurrentCorrelative { get; set; }
        public int PendingInvoices { get; set; }
        public string Prefix { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
