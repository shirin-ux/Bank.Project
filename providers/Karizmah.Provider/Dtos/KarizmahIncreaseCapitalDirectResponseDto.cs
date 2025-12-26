

namespace Karizmah.Provider.Dtos
{
   public class KarizmahIncreaseCapitalDirectResponseDto:BaseResponse<KarizmahIncreaseCapitalDirectResponseDto>
    {
        public Guid Id { get; set; } = default!;
        public long traceId { get; set; }
        public bool isRepeated { get; set; }
    }
}
