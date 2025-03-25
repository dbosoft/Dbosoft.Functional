using System;
using System.Collections.Generic;
using System.Text;
using LanguageExt.Common;

namespace Dbosoft.Functional;

#nullable enable

public static class Prelude
{
    public static string printError(Error error) =>
        error.Print();
}
