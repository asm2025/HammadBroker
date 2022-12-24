using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HammadBroker.Web.Helpers.TagHelpers;

[HtmlTargetElement("vid-asset")]
public class AssetVideoTagHelper : TagHelper
{
	private static readonly Lazy<IReadOnlySet<string>> __props = new Lazy<IReadOnlySet<string>>(() => typeof(AssetImageTagHelper)
																									.GetProperties(essentialMix.Constants.BF_PUBLIC_INSTANCE)
																									.Where(e => e.CustomAttributes.Any(p => typeof(HtmlAttributeNameAttribute).IsAssignableFrom(p.AttributeType)))
																									.Select(e => e.Name)
																									.ToHashSet(StringComparer.OrdinalIgnoreCase), LazyThreadSafetyMode.PublicationOnly);

	private string _videoId;
	private string _class;
	private string _videoClass;

	public AssetVideoTagHelper()
	{
	}

	[HtmlAttributeName(nameof(VideoId))]
	public string VideoId
	{
		get => _videoId;
		set => _videoId = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(AutoPlay))]
	public bool AutoPlay { get; set; }

	[HtmlAttributeName(nameof(Class))]
	public string Class
	{
		get => _class;
		set => _class = value.ToNullIfEmpty();
	}

	[HtmlAttributeName(nameof(VideoClass))]
	public string VideoClass
	{
		get => _videoClass;
		set => _videoClass = value.ToNullIfEmpty();
	}

	public override void Process([NotNull] TagHelperContext context, [NotNull] TagHelperOutput output)
	{
		string uri = GetVideo(VideoId);

		if (uri == null)
		{
			output.SuppressOutput();
			return;
		}

		IReadOnlySet<string> excludedAttributes = __props.Value;
		TagHelperAttributeList attributes = output.Attributes;
		output.TagName = "div";
		output.TagMode = TagMode.StartTagAndEndTag;
		if (!string.IsNullOrEmpty(Class)) output.Attributes.Add("class", Class);

		foreach (TagHelperAttribute attribute in context.AllAttributes.Where(e => !excludedAttributes.Contains(e.Name)))
			attributes.Add(attribute.Name, attribute.Value);

		TagBuilder iframe = new TagBuilder("iframe")
		{
			TagRenderMode = TagRenderMode.Normal,
			Attributes =
			{
				{"src", uri},
				{"allowfullscreen", null}
			}
		};
		if (!string.IsNullOrEmpty(VideoClass)) iframe.MergeAttribute("class", VideoClass);
		iframe.GenerateId(StringHelper.RandomKey(8), "_");
		output.Content.AppendHtml(iframe);
	}

	private string GetVideo(string id)
	{
		return string.IsNullOrEmpty(id)
					? null
					: $"https://www.youtube.com/embed/{VideoId}{(AutoPlay ? "?autoplay=1&mute=1" : null)}";
	}
}