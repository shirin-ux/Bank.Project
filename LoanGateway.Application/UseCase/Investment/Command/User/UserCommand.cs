namespace LoanService.Application.UseCase.Investment.Command.User;

public class UserCommand
{
    public string NationalCode { get; init; }
    public string BirthDate { get; init; }
    public string? PostalCode { get; init; }
    public string? MobileNumber { get; init; }
    public string? Address { get; init; }
}
