using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get;  set; }
        public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; protected set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; protected set; } = default!;


        protected void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
