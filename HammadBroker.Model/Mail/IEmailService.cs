using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace HammadBroker.Model.Mail;

public interface IEmailService : IEmailSender
{
	[NotNull]
	Task SendEmailAsync([NotNull] BasicEmail email);
}