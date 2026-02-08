using System;
using System.Collections.Generic;
using System.Text;

namespace EDA.DOMAIN.Entities
{
    public class Provider
    {
        public int Id { get; init; }
        public string Name { get; set; } = null!;
        public string RTN { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; } = null!;
        public string? WebsiteUrl { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? ModificationDate { get; set; }
        public string? ModificatedBy { get; set; }
        public bool IsActive { get; set; }
        public int TypeProviderId { get; set; }
    }
}
