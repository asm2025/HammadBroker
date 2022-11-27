using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace HammadBroker.Web.Areas.Admin.Controllers;

public class HomeController : Controller
{
	[NotNull]
	public IActionResult Index()
	{
		return View();
	}
}