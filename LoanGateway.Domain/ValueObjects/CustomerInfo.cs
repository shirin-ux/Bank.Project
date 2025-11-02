using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    public sealed record CustomerInfo(
        string NationalCode,
        DateTime? BirthDate,
        string? Mobile,
        string? PostalCode,
        int? Gender // 0/1
    );
}
