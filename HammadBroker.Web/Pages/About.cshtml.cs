using HammadBroker.Model.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HammadBroker.Web.Pages;

public class AboutModel : PageModel
{
	public AboutModel([NotNull] CompanyInfo companyInfo)
	{
		CompanyInfo = companyInfo;
	}

	[NotNull]
	public CompanyInfo CompanyInfo { get; }

	public void OnGet()
	{
	}
}