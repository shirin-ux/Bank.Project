using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karizmah.Provider.Dtos
{
    public sealed class ChindxIndexValueRequestDto
    {
       
        public string InstrumentId { get; init; } 


        public string FromDateKey { get; init; } = default!;
        public string ToDateKey { get; init; } = default!;


        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }

        public int Size { get; init; } = 10;
        public int Offset { get; init; } = 0;
    }

}
