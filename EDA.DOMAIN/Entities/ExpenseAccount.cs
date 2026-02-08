using System;
using System.Collections.Generic;
using System.Text;

namespace EDA.DOMAIN.Entities
{
    public class ExpenseAccount
    {
        public int Id { get; init; }
        public string Name { get; set; } = null!;
    }
}
