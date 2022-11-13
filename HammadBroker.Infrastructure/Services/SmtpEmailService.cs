using System.Net.Mail;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using HammadBroker.Model.Mail;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Infrastructure.Services;

public class SmtpEmailService : Disposable, IEmailService
{
	private readonly ILogger<SmtpEmailService> _logger;
	private readonly string _defaultFrom;

	private SmtpClient _client;

	public SmtpEmailService([NotNull] SmtpConfiguration configuration, [NotNull] ILogger<SmtpEmailService> logger)
	{
		_logger = logger;
		_defaultFrom = configuration.From.ToNullIfEmpty() ?? configuration.Login;
		_client = new SmtpClient
		{
			Host = configuration.Host,
			Port = configuration.Port,
			DeliveryMethod = SmtpDeliveryMethod.Network,
			EnableSsl = configuration.UseSSL
		};

		if (!string.IsNullOrEmpty(configuration.Password))
			_client.Credentials = new System.Net.NetworkCredential(configuration.Login, configuration.Password);
		else
			_client.UseDefaultCredentials = true;
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing) ObjectHelper.Dispose(ref _client);
		base.Dispose(disposing);
	}

	[NotNull]
	public Task SendEmailAsync([NotNull] string email, [NotNull] string subject, [NotNull] string htmlMessage)
	{
		MailMessage mail = new MailMessage(_defaultFrom, email)
		{
			Subject = subject,
			IsBodyHtml = true,
			Body = htmlMessage
		};
		_client.Send(mail);
		_logger.LogDebug(@$"Sent email:
to: {email},
subject: {subject}
message:
{htmlMessage}");
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task SendEmailAsync(BasicEmail email)
	{
		email.From = email.From.ToNullIfEmpty() ?? _defaultFrom;
		MailMessage mail = new MailMessage(email.From, email.To)
		{
			Subject = email.Subject,
			IsBodyHtml = email.IsBodyHtml,
			Body = email.Body
		};
		_client.Send(mail);
		_logger.LogDebug(@$"Sent email: 
from: {email.From},
to: {email.To},
subject: {email.Subject}
message (html? {email.IsBodyHtml.ToYesNo()}):
{email.Body}");
		return Task.CompletedTask;
	}
}
