namespace Karizmah.Provider;

public sealed class KarizmahInvestmentOptions
{
    public const string SectionName = "KarizmahInvestment";
    public string BaseUrlApi { get; set; } = default!;
    public string CreatePolicyWithoutInitialPaymentEndpoint { get; set; } = default!;
    public string TraceIdEndpoint { get; set; } = default!;
    public string DirectEndpoint { get; set; } = default!;
    public string BuyEndpoint { get; set; } = default!;
    public string IncreaseCapitalDirectEndpoint { get; set; } = default!;
    public string IncreaseEndpoint { get; set; } = default!;
    public string DecreaseDirectEndpoint { get; set; } = default!;
    public string BaseUrlToken { get; set; } = default!;
    public string CallbackBaseUrl { get; set; } = default!;
    public string DecreaseVerifyEndpoint { get; set; } = default!;
    public string SwapEnspoint { get; set; } = default!;
    public string OrderTransactionEnspoint { get; set; } = default!;
    public string RevokableAmountEndpoint { get; set; } = default!;
    public string OrderEnspoint { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string GrantType { get; set; } = default!;

    public string agentId { get; set; } = default!;

}

