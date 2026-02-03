using System;
using System.Collections.Generic;
using System.Text;

namespace EDA.DOMAIN.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Barcode { get; set; }
        public DateTime? Date { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int FamilyId { get; set; }
        public Family? Family { get; set; }
        public int TaxId { get; set; }
        public Tax? Tax { get; set; }
    }
}
