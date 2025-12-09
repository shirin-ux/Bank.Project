using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.Entities;

    public sealed class RefreshToken:BaseEntity
    {
        public Guid UserId { get; set; }
        public byte[] TokenHash { get; set; } = default!;
        public Guid JwtId { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedReason { get; set; }
    }

