using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment;

public class InvestmentPlans : BaseEntity
{
    public InvestmentPlanType PlanType { get;  set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; }
    public decimal MinAmount { get; private set; }

    public decimal? FixedIncomeAnnualRatePercent { get; set; }

    private readonly List<InvestmentPlanFeature> _features = new();
    private readonly List<InvestmentPlanFaq> _faqs = new();

    public IReadOnlyCollection<InvestmentPlanFeature> Features => _features;
    public IReadOnlyCollection<InvestmentPlanFaq> Faqs => _faqs;

    public void AddFeature(InvestmentPlanFeature feature) => _features.Add(feature);
    public void AddFaq(InvestmentPlanFaq faq) => _faqs.Add(faq);

}
