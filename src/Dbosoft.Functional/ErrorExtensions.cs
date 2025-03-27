using System;
using LanguageExt.Common;

namespace Dbosoft.Functional;

#nullable enable

public static class ErrorExtensions
{
    public static string Print(this Error error) =>
        error switch
        {
            ManyErrors manyErrors => string.Join(Environment.NewLine, manyErrors.Errors.Map(Print)),
            Exceptional exceptional => exceptional.ToException().ToString(),
            _ => error.Message + error.Inner.Map(inner => $"{Environment.NewLine}{Print(inner)}").IfNone(""),
        };
}
