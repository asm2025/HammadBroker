using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using essentialMix.Core.Web.VirtualPath;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Model;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace HammadBroker.Web.TagHelpers;

[HtmlTargetElement("img-asset")]
public class AssetImageTagHelper : TagHelper
{
	private static readonly Lazy<IReadOnlySet<string>> __props = new Lazy<IReadOnlySet<string>>(() => typeof(AssetImageTagHelper)
																									.GetProperties(essentialMix.Constants.BF_PUBLIC_INSTANCE)
																									.Where(e => e.CustomAttributes.Any(p => typeof(HtmlAttributeNameAttribute).IsAssignableFrom(p.AttributeType)))
																									.Select(e => e.Name)
																									.ToHashSet(StringComparer.OrdinalIgnoreCase), LazyThreadSafetyMode.PublicationOnly);

	private readonly IHttpContextAccessor _contextAccessor;
	private readonly string _basePath;
	private readonly string _placeholderImage;

	private string _name;
	private string _src;
	private string _alt = string.Empty;
	private string _class;
	private string _imageClass;
	private string _onError;
	private string _inputClass;
	private string _accept;

	public AssetImageTagHelper([NotNull] IHttpContextAccessor contextAccessor, [NotNull] IConfiguration configuration, [NotNull] VirtualPathSettings virtualPathSettings)
	{
		_contextAccessor = contextAccessor;
		PathContent assetsPath = virtualPathSettings.PathContents?.FirstOrDefault(e => e.Alias.IsSame("AssetImages")) ?? throw new ConfigurationErrorsException($"{nameof(VirtualPathSettings)} does not contain a definition for AssetImages.");
		_basePath = assetsPath.RequestPath.Trim('/');
		_placeholderImage = configuration.GetValue("Images:AssetPlaceholder", string.Empty);
	}

	[HtmlAttributeName(nameof(Name))]
	public string Name
	{
		get => _name;
		set => _name = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(Src))]
	public string Src
	{
		get => _src;
		set => _src = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(Alt))]
	public string Alt
	{
		get => _alt;
		set => _alt = value.ToNullIfEmpty() ?? string.Empty;
	}

	[HtmlAttributeName(nameof(Class))]
	public string Class
	{
		get => _class;
		set => _class = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(ImageClass))]
	public string ImageClass
	{
		get => _imageClass;
		set => _imageClass = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(OnError))]
	public string OnError
	{
		get => _onError;
		set => _onError = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(InputClass))]
	public string InputClass
	{
		get => _inputClass;
		set => _inputClass = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(Accept))]
	public string Accept
	{
		get => _accept;
		set => _accept = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(ReadOnly))]
	public bool ReadOnly { get; set; }

	public override void Process([NotNull] TagHelperContext context, [NotNull] TagHelperOutput output)
	{
		string uri = GetImage(Src);
		IReadOnlySet<string> excludedAttributes = __props.Value;
		TagHelperAttributeList attributes = output.Attributes;

		if (ReadOnly)
		{
			output.TagName = "img";
			output.TagMode = TagMode.SelfClosing;
			if (!string.IsNullOrEmpty(Name)) attributes.Add("name", Name);
			attributes.Add("src", $"{uri}?v={DateTime.Now.Ticks}");
			attributes.Add("alt", Alt);
			if (!string.IsNullOrEmpty(ImageClass)) attributes.Add("class", ImageClass);
			attributes.Add("onerror", BuildErrorHandler());

			foreach (TagHelperAttribute attribute in context.AllAttributes.Where(e => !excludedAttributes.Contains(e.Name)))
				attributes.Add(attribute.Name, attribute.Value);

			return;
		}

		output.TagName = "div";
		output.TagMode = TagMode.StartTagAndEndTag;
		if (!string.IsNullOrEmpty(Class)) output.Attributes.Add("class", Class);

		foreach (TagHelperAttribute attribute in context.AllAttributes.Where(e => !excludedAttributes.Contains(e.Name)))
			attributes.Add(attribute.Name, attribute.Value);

		TagBuilder img = new TagBuilder("img")
		{
			TagRenderMode = TagRenderMode.SelfClosing,
			Attributes =
			{
				{"src", $"{uri}?v={DateTime.Now.Ticks}"},
				{"alt", Alt},
				{"onerror", BuildErrorHandler()}
			}
		};
		if (!string.IsNullOrEmpty(ImageClass)) img.MergeAttribute("class", ImageClass);
		img.GenerateId(StringHelper.RandomKey(8), "_");

		TagBuilder input = new TagBuilder("input")
		{
			TagRenderMode = TagRenderMode.SelfClosing,
			Attributes =
			{
				{"type", "file"},
				{"accept", Accept ?? Constants.Images.Extensions},
				{"oninput", BuildInputHandler(img.Attributes["id"], uri)}
			}
		};

		if (!string.IsNullOrEmpty(Name)) input.MergeAttribute("name", Name, true);
		if (!string.IsNullOrEmpty(InputClass)) input.MergeAttribute("class", InputClass, true);

		output.Content.AppendHtml(img);
		output.Content.AppendHtml(input);
	}

	[NotNull]
	private string GetImage(string fileName)
	{
		if (string.IsNullOrEmpty(fileName)) return string.Empty;
		HttpContext context = _contextAccessor.HttpContext;
		if (context == null) return string.Empty;
		HttpRequest request = context.Request;
		return $"{request.Scheme}://{request.Host}/{_basePath}/{fileName}";
	}

	[NotNull]
	private string BuildErrorHandler()
	{
		string onError = OnError;
		if (!string.IsNullOrEmpty(onError)) return onError;
		return $"this.onerror=null;this.src='{GetImage(_placeholderImage)}'";
	}

	[NotNull]
	private static string BuildInputHandler(string id, string url)
	{
		return $"previewImage(this, document.getElementById('{id}'), '{url}');";
	}
}