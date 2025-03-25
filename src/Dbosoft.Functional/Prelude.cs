using LanguageExt.Common;

namespace Dbosoft.Functional;

#nullable enable

public static class Prelude
{
    public static string printError(Error error) =>
        error.Print();
}
