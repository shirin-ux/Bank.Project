

namespace Karizmah.Provider.Dtos
{
   public class KarizmahIncreaseResponseDto:BaseResponse<KarizmahIncreaseResponseDto>
    {
        public Guid Id { get; set; } = default!;
        public long traceId { get; set; }
        public bool isRepeated { get; set; }
    }
}
