namespace Karizmah.Provider.Dtos;

public sealed class ChindxIndexValueResponseDto
{
    public string Status { get; set; } = default!;
    public int StatusCode { get; set; }
    public string? Message { get; set; }

    public ChindxIndexValueDataDto Data { get; set; } = default!;
    public ChindxMetaDto Meta { get; set; } = default!;


    public List<object> Errors { get; set; } = new();

    public DateTimeOffset Timestamp { get; set; }
    public string RequestId { get; set; } = default!;
}

public sealed class ChindxIndexValueDataDto
{
    public string InstrumentId { get; set; } = default!;
    public List<ChindxIndexPointDto> IndexValue { get; set; } = new();
}

public sealed class ChindxIndexPointDto
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
}

public sealed class ChindxMetaDto
{
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public string Timezone { get; set; } = default!;
    public string Sort { get; set; } = default!;
    public string Order { get; set; } = default!;
    public ChindxPaginationDto Pagination { get; set; } = default!;
}

public sealed class ChindxPaginationDto
{
    public int Size { get; set; }
    public int Offset { get; set; }
    public int Total { get; set; }
    public bool HasMore { get; set; }
}
