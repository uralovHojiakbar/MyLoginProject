using Application.Common.Interfaces;

namespace Infrastructure.Emailing;

public class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        // MOCK: real email o‘rniga Console’ga chiqaramiz
        Console.WriteLine("===== EMAIL (MOCK) =====");
        Console.WriteLine($"TO: {toEmail}");
        Console.WriteLine($"SUBJECT: {subject}");
        Console.WriteLine($"BODY:\n{body}");
        Console.WriteLine("========================");
        return Task.CompletedTask;
    }
}
