using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.TransferInquiry;

public sealed record TransferInquiryResultDto:IBankResponse
{
    public string RegisterCode { get; init; } = default!;
    public decimal ApprovalCode { get; init; }
    public int? MessageCode { get; init; }
    public string? Message { get; init; }

    public IReadOnlyList<InquiryDetailDto> InquiryDetails { get; init; } = Array.Empty<InquiryDetailDto>();
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    int? IBankResponse.MessageCode => throw new NotImplementedException();

    string? IBankResponse.Message => throw new NotImplementedException();

    string IBankResponse.RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public sealed record InquiryDetailDto
    {
        public TransferStatus transferStatus { get; set; } // 0..6
        public decimal PayAmount { get; init; }
        public string DestIban { get; init; } = default!;
        public string DestName { get; init; } = default!;
        public string DestNationalId { get; init; } = default!;
        public int TransType { get; init; }  
        public string Description { get; init; } = default!;

        public int? SendDate { get; init; }            
        public int? SendTime { get; init; }        
        public int? ReturnDate { get; init; }         
        public int? ReturnTime { get; init; }         
        public int? DeleteDate { get; init; }            
        public int? DeleteTime { get; init; }    

        public string? TrackingNo { get; init; }          
        public long? ReturnReasonCode { get; init; }   
    }
    public enum TransferStatus : short
    {
        Registered = 0,   // ثبت شده
        Deleted = 1,      // حذف
        Returned = 2,     // برگشتی
        Sent = 3,         // ارسال به مقصد
        Unsendable = 4,   // عدم امکان ارسال
        CanceledNoDebit = 5, // لغو بدون برداشت
        CanceledAndReturned = 6 // لغو و برگشت
    }

    IEnumerable<BankStatusItem> IBankResponse.GetStatusItems()
    {
        throw new NotImplementedException();
    }
}
