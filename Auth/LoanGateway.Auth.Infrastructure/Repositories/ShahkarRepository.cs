using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public class ShahkarRepository : IShahkarRepository
{
    private readonly AuthDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ShahkarRepository(AuthDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task InsertAsync(VerfiyMobileOwnerInquiry log, CancellationToken cancellationToken = default)
    {
        if (log.Id == Guid.Empty)
            log.Id = Guid.NewGuid();

        _context.VerfiyMobileOwnerInquiries.Add(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
