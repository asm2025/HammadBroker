using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using NWebsec.Core.Common.Middleware.Options;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Extensions;

public static class IApplicationBuilderExtension
{
    [NotNull]
    public static IApplicationBuilder UseSecurityHeaders([NotNull] this IApplicationBuilder thisValue, [NotNull] IConfiguration configuration)
    {
        ForwardedHeadersOptions forwardingOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All
        };

        forwardingOptions.KnownNetworks.Clear();
        forwardingOptions.KnownProxies.Clear();

        thisValue.UseForwardedHeaders(forwardingOptions);

        thisValue.UseReferrerPolicy(options => options.NoReferrer());

        // CSP Configuration to be able to use external resources
        string[] cspTrustedDomains = configuration.GetSection(Model.Constants.Configuration.CspTrustedDomainsKey).Get<string[]>();
        thisValue.UseCsp(csp =>
        {
            csp.Sandbox(options =>
                {
                    options.AllowSameOrigin()
                            .AllowScripts()
                            .AllowForms()
                            .AllowModals()
                            .AllowPopups()
                            .AllowPopupsToEscapeSandbox();
                })
                .FrameAncestors(options =>
                {
                    options.None();
                })
                .BaseUris(options =>
                {
                    options.Self();
                })
                .ObjectSources(options =>
                {
                    options.None();
                });

            if (cspTrustedDomains is { Length: > 0 })
            {
                csp.ImageSources(options =>
                    {
                        options.CustomSources(cspTrustedDomains.Prepend("data:").ToArray())
                                .Self();
                    })
                    .FontSources(options =>
                    {
                        options.CustomSources(cspTrustedDomains)
                                .Self();
                    })
                    .ScriptSources(options =>
                    {
                        options.CustomSources(cspTrustedDomains)
                                .Self()
                                .UnsafeInline()
                                .UnsafeEval();
                    })
                    .StyleSources(options =>
                    {
                        options.CustomSources(cspTrustedDomains)
                                .Self()
                                .UnsafeInline();
                    })
                    .DefaultSources(options =>
                    {
                        options.CustomSources(cspTrustedDomains)
                                .Self();
                    });
            }
            else
            {
                csp.ImageSources(options =>
                {
                    options.CustomSources("data:")
                            .Self();
                });
            }

            AddTrustedConnectDomains(csp, configuration);
        });
        return thisValue;
    }

    [Conditional("DEBUG")]
    private static void AddTrustedConnectDomains([NotNull] IFluentCspOptions csp, [NotNull] IConfiguration configuration)
    {
        string[] cspTrustedConnectDomains = configuration.GetSection(Model.Constants.Configuration.CspTrustedConnectDomainsKey).Get<string[]>();
        if (cspTrustedConnectDomains is not { Length: > 0 }) return;
        csp.ConnectSources(options =>
        {
            options.CustomSources(cspTrustedConnectDomains)
                    .Self();
        });
    }
}