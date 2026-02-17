using LanguageExt;
using LanguageExt.Common;

using static LanguageExt.Prelude;
using static LanguageExt.CompatPrelude;

#pragma warning disable CS0618 // Obsolete shim types

namespace Dbosoft.Functional.Tests.Compat;

public class EitherAsyncTests
{
    [Fact]
    public async Task Right_CreatesRightValue()
    {
        var either = EitherAsync<Error, int>.Right(42);
        var result = await either.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task Left_CreatesLeftValue()
    {
        var error = Error.New("test error");
        var either = EitherAsync<Error, int>.Left(error);
        var result = await either.ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("test error");
    }

    [Fact]
    public async Task RightAsync_CreatesRightValue()
    {
        var either = RightAsync<Error, int>(42);
        var result = await either.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task LeftAsync_CreatesLeftValue()
    {
        var error = Error.New("test error");
        var either = LeftAsync<Error, int>(error);
        var result = await either.ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("test error");
    }

    [Fact]
    public async Task ImplicitConversion_FromEither_Works()
    {
        Either<Error, int> sync = Right<Error, int>(42);
        EitherAsync<Error, int> async_ = sync;
        var result = await async_.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task Map_RightValue_TransformsValue()
    {
        var either = RightAsync<Error, int>(21);
        var mapped = either.Map(x => x * 2);
        var result = await mapped.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task Map_LeftValue_PreservesLeft()
    {
        var error = Error.New("test error");
        var either = LeftAsync<Error, int>(error);
        var mapped = either.Map(x => x * 2);
        var result = await mapped.ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("test error");
    }

    [Fact]
    public async Task MapLeft_LeftValue_TransformsLeft()
    {
        var either = LeftAsync<string, int>("original");
        var mapped = either.MapLeft(e => $"mapped: {e}");
        var result = await mapped.ToEither();

        result.Should().BeLeft().Which.Should().Be("mapped: original");
    }

    [Fact]
    public async Task MapLeft_RightValue_PreservesRight()
    {
        var either = RightAsync<string, int>(42);
        var mapped = either.MapLeft(e => $"mapped: {e}");
        var result = await mapped.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task Bind_RightToRight_ChainsSuccessfully()
    {
        var either = RightAsync<Error, int>(21);
        var bound = either.Bind(x => RightAsync<Error, int>(x * 2));
        var result = await bound.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task Bind_RightToLeft_ReturnsLeft()
    {
        var either = RightAsync<Error, int>(21);
        var bound = either.Bind(_ => LeftAsync<Error, int>(Error.New("bind failed")));
        var result = await bound.ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("bind failed");
    }

    [Fact]
    public async Task Bind_LeftValue_ShortCircuits()
    {
        var either = LeftAsync<Error, int>(Error.New("original"));
        var bindCalled = false;
        var bound = either.Bind(x =>
        {
            bindCalled = true;
            return RightAsync<Error, int>(x * 2);
        });
        var result = await bound.ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("original");
        bindCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Match_RightValue_CallsRightHandler()
    {
        var either = RightAsync<Error, int>(42);
        var result = await either.Match(
            Right: r => $"right: {r}",
            Left: l => $"left: {l.Message}");

        result.Should().Be("right: 42");
    }

    [Fact]
    public async Task Match_LeftValue_CallsLeftHandler()
    {
        var either = LeftAsync<Error, int>(Error.New("test error"));
        var result = await either.Match(
            Right: r => $"right: {r}",
            Left: l => $"left: {l.Message}");

        result.Should().Be("left: test error");
    }

    [Fact]
    public async Task Select_RightValue_TransformsValue()
    {
        var either = RightAsync<Error, int>(21);
        var selected = either.Select(x => x * 2);
        var result = await selected.ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task LinqQuery_MultipleBinds_ChainsCorrectly()
    {
        var result = await (
            from a in RightAsync<Error, int>(10)
            from b in RightAsync<Error, int>(20)
            from c in RightAsync<Error, int>(12)
            select a + b + c
        ).ToEither();

        result.Should().BeRight().Which.Should().Be(42);
    }

    [Fact]
    public async Task LinqQuery_SecondFails_ReturnsFirstError()
    {
        var result = await (
            from a in RightAsync<Error, int>(10)
            from b in LeftAsync<Error, int>(Error.New("second failed"))
            from c in RightAsync<Error, int>(12)
            select a + b + c
        ).ToEither();

        result.Should().BeLeft().Which.Message.Should().Be("second failed");
    }

    [Fact]
    public async Task ToAff_RightValue_ConvertsToSucc()
    {
        var either = RightAsync<Error, int>(42);
        var aff = either.ToAff(e => e);
        var fin = await aff.Run();

        fin.IsSucc.Should().BeTrue();
        ((int)fin).Should().Be(42);
    }

    [Fact]
    public async Task ToAff_LeftValue_ConvertsToFail()
    {
        var either = LeftAsync<Error, int>(Error.New("test error"));
        var aff = either.ToAff(e => e);
        var fin = await aff.Run();

        fin.IsFail.Should().BeTrue();
    }

    [Fact]
    public async Task ToAff_WithLeftMapping_MapsError()
    {
        var either = LeftAsync<string, int>("raw error");
        var aff = either.ToAff(e => Error.New($"mapped: {e}"));
        var fin = await aff.Run();

        fin.IsFail.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncFlow_RealAsyncTask_WorksCorrectly()
    {
        var either = new EitherAsync<Error, int>(
            Task.Run(async () =>
            {
                await Task.Delay(10);
                return Right<Error, int>(42);
            }));

        var result = await either
            .Map(x => x + 8)
            .Bind(x => EitherAsync<Error, int>.Right(x * 2))
            .ToEither();

        result.Should().BeRight().Which.Should().Be(100);
    }

    [Fact]
    public async Task AsyncFlow_ChainedAsyncOperations_ExecuteInOrder()
    {
        var executionOrder = new List<string>();

        var either = new EitherAsync<Error, int>(
            Task.Run(async () =>
            {
                executionOrder.Add("source");
                await Task.Delay(10);
                return Right<Error, int>(1);
            }));

        var chained = either
            .Map(x =>
            {
                executionOrder.Add("map");
                return x + 1;
            })
            .Bind(x =>
            {
                executionOrder.Add("bind");
                return EitherAsync<Error, int>.Right(x + 1);
            });

        var result = await chained.ToEither();

        result.Should().BeRight().Which.Should().Be(3);
        executionOrder.Should().ContainInOrder("source", "map", "bind");
    }
}
