using LanguageExt;
using LanguageExt.Common;

using static LanguageExt.Prelude;
using static LanguageExt.CompatPrelude;

#pragma warning disable CS0618 // Obsolete shim types

namespace Dbosoft.Functional.Tests.Compat;

public class AffTests
{
    [Fact]
    public async Task Run_SuccessfulEffect_ReturnsSucc()
    {
        var aff = RightAsync<Error, int>(42).ToAff();
        var fin = await aff.Run();

        fin.IsSucc.Should().BeTrue();
        ((int)fin).Should().Be(42);
    }

    [Fact]
    public async Task Run_FailedEffect_ReturnsFail()
    {
        var aff = LeftAsync<Error, int>(Error.New("effect failed")).ToAff();
        var fin = await aff.Run();

        fin.IsFail.Should().BeTrue();
    }

    [Fact]
    public async Task EitherAsync_ToAff_ToEitherAsync_RoundTrip()
    {
        var original = RightAsync<Error, int>(42);

        // EitherAsync → Aff → run → Fin → EitherAsync → Either
        var aff = original.ToAff();
        var fin = await aff.Run();

        fin.IsSucc.Should().BeTrue();

        var finTask = new ValueTask<Fin<int>>(fin);
        var backToEither = finTask.ToEitherAsync();
        var result = await backToEither.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task EitherAsync_ToAff_ToEitherAsync_RoundTrip_WithError()
    {
        var original = LeftAsync<Error, int>(Error.New("test error"));

        var aff = original.ToAff();
        var fin = await aff.Run();

        fin.IsFail.Should().BeTrue();

        var finTask = new ValueTask<Fin<int>>(fin);
        var backToEither = finTask.ToEitherAsync();
        var result = await backToEither.ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("test error");
    }

    [Fact]
    public async Task AsyncFlow_EitherAsyncMapThenToAff_Works()
    {
        var result = await RightAsync<Error, int>(10)
            .Map(x => x * 2)
            .Map(x => x + 1)
            .Bind(x => RightAsync<Error, int>(x + 1))
            .ToAff()
            .Run();

        result.IsSucc.Should().BeTrue();
        ((int)result).Should().Be(22);
    }

    [Fact]
    public async Task AsyncFlow_FailureInChain_PropagatesError()
    {
        var result = await RightAsync<Error, int>(10)
            .Map(x => x * 2)
            .Bind(_ => LeftAsync<Error, int>(Error.New("chain broke")))
            .Map(x => x + 1) // should not execute
            .ToAff()
            .Run();

        result.IsFail.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncFlow_LinqSyntax_ThroughAff()
    {
        var either =
            from a in RightAsync<Error, int>(10)
            from b in RightAsync<Error, int>(20)
            select a + b;

        var fin = await either.ToAff().Run();

        fin.IsSucc.Should().BeTrue();
        ((int)fin).Should().Be(30);
    }

    [Fact]
    public async Task ToAff_WithCustomLeftMapping_MapsError()
    {
        var either = LeftAsync<string, int>("raw error");
        var aff = either.ToAff(e => Error.New($"mapped: {e}"));
        var fin = await aff.Run();

        fin.IsFail.Should().BeTrue();
    }

    [Fact]
    public async Task ToAff_WithCustomLeftMapping_RightValue_Succeeds()
    {
        var either = RightAsync<string, int>(42);
        var aff = either.ToAff(e => Error.New($"mapped: {e}"));
        var fin = await aff.Run();

        fin.IsSucc.Should().BeTrue();
        ((int)fin).Should().Be(42);
    }
}
