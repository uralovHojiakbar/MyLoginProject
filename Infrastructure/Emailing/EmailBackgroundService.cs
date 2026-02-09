using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Emailing;

public sealed class EmailBackgroundService : BackgroundService
{
    private readonly ChannelReader<EmailMessage> _reader;
    private readonly IEmailSender _sender;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        Channel<EmailMessage> channel,
        IEmailSender sender,
        ILogger<EmailBackgroundService> logger)
    {
        _reader = channel.Reader;
        _sender = sender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _sender.SendAsync(msg.ToEmail, msg.Subject, msg.Body, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email send failed (mock). To={ToEmail}", msg.ToEmail);
            }
        }
    }
}
