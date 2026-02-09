using System.Threading.Channels;
using Application.Common.Interfaces;

namespace Infrastructure.Emailing;

public class InMemoryEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public InMemoryEmailQueue(Channel<EmailMessage> channel)
    {
        _channel = channel;
    }

    public void Enqueue(string toEmail, string subject, string body)
    {
        // fire-and-forget enqueue (async processing)
        _channel.Writer.TryWrite(new EmailMessage(toEmail, subject, body));
    }
}
