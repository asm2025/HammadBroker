using System.Configuration;
using System.Linq;
using essentialMix.Extensions;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace HammadBroker.Web.Helpers.TagHelpers;

[HtmlTargetElement("img-avatar")]
public class AvatarTagHelper : TagHelper
{
	private readonly IHttpContextAccessor _contextAccessor;
	private readonly string _basePath;
	private readonly string _placeholderImage;

	private string _email;
	private string _alt;
	private string _class;

	public AvatarTagHelper([NotNull] IHttpContextAccessor contextAccessor, [NotNull] IConfiguration configuration, [NotNull] VirtualPathSettings virtualPathSettings)
	{
		_contextAccessor = contextAccessor;
		PathContent assetsPath = virtualPathSettings.PathContents?.FirstOrDefault(e => e.Alias.IsSame("UserImages")) ?? throw new ConfigurationErrorsException($"{nameof(VirtualPathSettings)} does not contain a definition for UserImages.");
		_basePath = assetsPath.RequestPath;
		_placeholderImage = configuration.GetValue("Images:UserPlaceholder", string.Empty);
	}

	[HtmlAttributeName("email")]
	public string Email
	{
		get => _email;
		set => _email = value.ToNullIfEmpty();
	}

	[HtmlAttributeName("alt")]
	public string Alt
	{
		get => _alt;
		set => _alt = value.ToNullIfEmpty();
	}

	[HtmlAttributeName("class")]
	public string Class
	{
		get => _class;
		set => _class = value.ToNullIfEmpty();
	}

	[HtmlAttributeName("size")]
	public int Size { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		const int SIZE = 16;

		if (string.IsNullOrEmpty(Email))
		{
			output.SuppressOutput();
			return;
		}

		output.TagName = "img";
		if (!string.IsNullOrEmpty(Class)) output.Attributes.Add("class", Class);
		output.Attributes.Add("alt", Alt ?? string.Empty);
		output.Attributes.Add("src", GetLink($"{_basePath}/{_placeholderImage}"));

		int size = Size.Within(0, 6) * SIZE;

		if (size > 0)
		{
			output.Attributes.Add("width", size);
			output.Attributes.Add("height", size);
		}

		output.TagMode = TagMode.SelfClosing;
	}

	[NotNull]
	private string GetBaseUrl()
	{
		HttpContext context = _contextAccessor.HttpContext;
		if (context == null) return string.Empty;
		HttpRequest request = context.Request;
		return $"{request.Scheme}://{request.Host}";
	}

	private string GetLink(string uri)
	{
		return uri?.Replace("~/", $"{GetBaseUrl()}/");
	}
}