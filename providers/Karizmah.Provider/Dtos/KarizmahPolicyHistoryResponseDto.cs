namespace Karizmah.Provider.Dtos;

public class KarizmahPolicyHistoryResponseDto : BaseResponse<KarizmahPolicyHistoryResponseDto>
{
    public List<KarizmahPolicyHistoryItemDto> Result { get; set; } = new();
    public class KarizmahPolicyHistoryItemDto
    {
        public int id { get; set; }
        public DateTimeOffset date { get; set; }
        public decimal value { get; set; }
        public decimal revokableAmount { get; set; }

        public decimal? penalty { get; set; }

        public decimal? transactionFee { get; set; }

        public decimal? loansValue { get; set; }

        public bool isActive { get; set; }
        public decimal pnlRatio { get; set; }
        public decimal pnlValue { get; set; }

        public decimal startValue { get; set; }

        public decimal endValue { get; set; }

        public string solutionType { get; set; } = default!;

        public string? insured { get; set; }
    }
}
