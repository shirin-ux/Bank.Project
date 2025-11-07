using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities;

public sealed class InquiryInfo
{
    public Guid Id { get; set; }
    public bool? Allowed { get; set; }
    public decimal? MaxApprovedAmount { get; set; }
    public int? Ics { get; set; }
    public Grade? IcsGrade { get; set; }
    public string? ExpireAt { get; set; }
    public List<StatusItem> Statuses { get; set; }
    public Guid LoanRequestId { get; set; }
    public class StatusItem
    {
        public string ResponseCode { get; set; }
        public string ResponseStatus { get; set; }
    }

}
