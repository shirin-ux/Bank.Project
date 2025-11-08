using Common;


namespace LoanService.Application.UseCase.Query.ReturnTransferReport;



public sealed record ReturnTransferReportResultDto:IBankResponse
{
    public List<ReturnedTransferDto> ReturnedTransfers { get; init; } 
    public decimal FromId { get; init; }      
    public int? MessageCode { get; init; }
    public string Message { get; init; }
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public sealed record ReturnedTransferDto
    {
        public string RegisterCode { get; init; } = default!;     
        public decimal ApprovalId { get; init; }                  
        public TransferStatus TransferStatus { get; init; }       
        public int? ReturnReasonCode { get; init; }               
        public string? ReturnReasonDesc { get; init; }            
        public string DestIban { get; init; } = default!;         
        public string DestName { get; init; } = default!;         
        public short? DestBankCode { get; init; }                 
        public decimal PayAmount { get; init; }                   
        public int? TransferDate { get; init; }                   
        public int? ReturnDate { get; init; }                     
        public int? ReturnTime { get; init; }                     
        public int? SendDate { get; init; }                       
        public int? SendTime { get; init; }                       
        public int? NoSendDate { get; init; }                     
        public short? ReasonCode { get; init; }                   
        public string SourceIban { get; init; } = default!;       
        public string? TrackingNo { get; init; }                  
        public int RowId { get; init; }                           
    }
    public enum TransferStatus : short
    {
        Registered = 0,
        Deleted = 1,
        Returned = 2,
        Sent = 3,
        Unsendable = 4,
        CanceledNoDebit = 5,
        CanceledAndReturned = 6
    }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }
}