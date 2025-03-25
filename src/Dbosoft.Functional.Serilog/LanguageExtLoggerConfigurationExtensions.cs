using System;
using LanguageExt.Common;
using Serilog;
using Serilog.Configuration;

namespace Dbosoft.Functional.Serilog;

/// <summary>
/// Extends <see cref="LoggerEnrichmentConfiguration"/> to add the
/// <see cref="LanguageExtEnricher"/> for LanguageExt.
/// </summary>
public static class LanguageExtLoggerConfigurationExtensions
{
    /// <summary>
    /// Enrich the log events with an <c>InnerError</c> property which contains
    /// the details of <see cref="ErrorException.Inner"/>.
    /// </summary>
    /// <remarks>
    /// The <c>InnerError</c> property needs to be added to the Serilog message
    /// template to be included in the log output. The template might look like this:
    /// <c>[{Level:u3}] {Message}{NewLine}{Exception}{InnerError}</c>.
    /// </remarks>
    public static LoggerConfiguration WithLanguageExt(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration is null)
            throw new ArgumentNullException(nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With<LanguageExtEnricher>();
    }
}
