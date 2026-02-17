using LanguageExt;
using LanguageExt.Common;

using static LanguageExt.Prelude;
using static LanguageExt.CompatPrelude;

#pragma warning disable CS0618 // Obsolete shim types

namespace Dbosoft.Functional.Tests;

public class AffExtensionsTests
{
    [Fact]
    public async Task ToAff_RightValue_ReturnsSucc()
    {
        var either = RightAsync<Error, int>(42);
        var aff = either.ToAff();
        var fin = await aff.Run();

        fin.IsSucc.Should().BeTrue();
        var value = (int)fin;
        value.Should().Be(42);
    }

    [Fact]
    public async Task ToAff_LeftValue_ReturnsFail()
    {
        var error = Error.New("test error");
        var either = LeftAsync<Error, int>(error);
        var aff = either.ToAff();
        var fin = await aff.Run();

        fin.IsFail.Should().BeTrue();
    }

    [Fact]
    public async Task ToEitherAsync_SuccFin_ReturnsRight()
    {
        Fin<int> succFin = 42;
        var fin = new ValueTask<Fin<int>>(succFin);
        var either = fin.ToEitherAsync();
        var result = await either.ToEither();

        result.IsRight.Should().BeTrue();
        result.IfRight(v => v.Should().Be(42));
    }

    [Fact]
    public async Task ToEitherAsync_FailFin_ReturnsLeft()
    {
        var error = Error.New("test error");
        Fin<int> failFin = error;
        var fin = new ValueTask<Fin<int>>(failFin);
        var either = fin.ToEitherAsync();
        var result = await either.ToEither();

        result.IsLeft.Should().BeTrue();
    }
}
