

using Bank.Mellat.Provider.Dtos;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Infrastructure.Extention;

    public static class CollateralTypeExtensions
    {
        public static string ToMellatValue(this CollateralType type)
            => type switch
            {
                CollateralType.CHEQUE => MellatCollateralTypes.Cheque,
                CollateralType.PROMISSORY => MellatCollateralTypes.PromissoryNote,
                _ => throw new NotSupportedException()
            };
    }

