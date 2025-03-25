using System;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using Serilog.Core;
using Serilog.Events;

namespace Dbosoft.Functional.Serilog;

/// <summary>
/// This <see cref="ILogEventEnricher"/> enriches the log events with an
/// <c>InnerError</c> property with the details of <see cref="ErrorException.Inner"/>
/// when an <see cref="Error"/> or <see cref="ErrorException"/> is logged. 
/// Without this enricher, only the top-level error message would be included in the log.
/// </summary>
public class LanguageExtEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent is null)
            throw new ArgumentNullException(nameof(logEvent));

        if (propertyFactory is null)
            throw new ArgumentNullException(nameof(propertyFactory));

        if (logEvent.Exception is ErrorException { Inner.IsSome: true } eex)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "InnerError",
                eex.Inner.ValueUnsafe().ToError().Print()));
        }
    }
}
