using System;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace HammadBroker.Web.TagHelpers;

[HtmlTargetElement(Attributes = "menu-route")]
public class ActiveMenuHelper : AnchorTagHelper
{
	/// <inheritdoc />
	public ActiveMenuHelper([NotNull] IHtmlGenerator generator)
		: base(generator)
	{
	}

	public override void Process(TagHelperContext context, [NotNull] TagHelperOutput output)
	{
		RouteValueDictionary routeData = ViewContext.RouteData.Values;
		(string area, string controller, string action, string page) = GetRouteData(routeData);
		(string carea, string ccontroller, string caction, string cpage) = FixData(Area, Controller, Action, Page);

		bool areaSame = AreEqual(carea, area);
		bool actionSame = AreEqual(caction, action) || AreEqual(cpage, page);
		string actionOrPage = page.ToNullIfEmpty() ?? action;
		bool controllerSame = actionSame || (string.IsNullOrEmpty(actionOrPage) && AreEqual(ccontroller, controller));
		bool result = areaSame && controllerSame && actionSame;

		string classes;

		if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute attribute))
		{
			classes = attribute.Value?.ToString() ?? string.Empty;
			output.Attributes.Remove(attribute);
		}
		else
		{
			classes = string.Empty;
		}

		if (result)
		{
			if (!classes.Contains("active")) classes = string.Join(' ', classes, "active");
		}
		else
		{
			if (classes.Contains("active"))
			{
				classes = classes.Replace("active", string.Empty)
								.Replace("  ", " ")
								.Trim();
			}
		}

		output.Attributes.Add("class", classes);
	}

	private static (string, string, string, string) GetRouteData([NotNull] RouteValueDictionary routeData)
	{
		return FixData(routeData["area"] as string, routeData["controller"] as string, routeData["action"] as string, routeData["page"] as string);
	}

	private static (string, string, string, string) FixData(string area, string controller, string action, string page)
	{
		if (string.IsNullOrEmpty(controller)) controller = "Home";
		page = FixPage(page);
		if (string.IsNullOrEmpty(action) && string.IsNullOrEmpty(page)) action = "Index";
		if (string.IsNullOrEmpty(page) && string.IsNullOrEmpty(action)) page = "Index";
		return (area, controller, action, page);
	}

	private static string FixPage(string page)
	{
		return string.IsNullOrEmpty(page) || (page[0] != '.' && page[0] == '/' && page[0] == '~')
					? page
					: page.Trim('.', '/', '~');
	}

	private static bool AreEqual(string a, string b)
	{
		if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return true;
		return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
	}
}