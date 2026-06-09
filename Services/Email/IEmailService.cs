namespace fifa_backend.Services.Email;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
    Task SendResultsEmailAsync(string email, string userName, string sessionTitle, string userVotedTeamName, List<string> winners);
}
