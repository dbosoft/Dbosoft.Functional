using LanguageExt;
using LanguageExt.Common;

using static LanguageExt.Prelude;

namespace Dbosoft.Functional.Tests;

public class EitherExtensionsTests
{
    [Fact]
    public void ThrowIfLeft_RightValue_ReturnsValue()
    {
        var either = Right<Error, int>(42);
        var result = either.ThrowIfLeft();

        result.Should().Be(42);
    }

    [Fact]
    public void ThrowIfLeft_LeftValue_ThrowsException()
    {
        var error = Error.New("test error");
        var either = Left<Error, int>(error);

        var act = () => either.ThrowIfLeft();
        act.Should().Throw<Exception>();
    }

    [Fact]
    public async Task NoneToError_SomeValue_ReturnsRight()
    {
        var either = RightAsync<Error, Option<int>>(Some(42));
        var result = await either.NoneToError(Error.New("not found")).ToEither();

        result.IsRight.Should().BeTrue();
        result.IfRight(v => v.Should().Be(42));
    }

    [Fact]
    public async Task NoneToError_NoneValue_ReturnsLeft()
    {
        var either = RightAsync<Error, Option<int>>(None);
        var result = await either.NoneToError(Error.New("not found")).ToEither();

        result.IsLeft.Should().BeTrue();
        result.IfLeft(e => e.Message.Should().Be("not found"));
    }

    [Fact]
    public async Task NoneToError_LeftValue_PreservesLeft()
    {
        var either = LeftAsync<Error, Option<int>>(Error.New("original error"));
        var result = await either.NoneToError(Error.New("not found")).ToEither();

        result.IsLeft.Should().BeTrue();
        result.IfLeft(e => e.Message.Should().Be("original error"));
    }

    [Fact]
    public async Task SomeToError_NoneValue_ReturnsUnit()
    {
        var either = RightAsync<Error, Option<int>>(None);
        var result = await either.SomeToError(Error.New("already exists")).ToEither();

        result.IsRight.Should().BeTrue();
    }

    [Fact]
    public async Task SomeToError_SomeValue_ReturnsLeft()
    {
        var either = RightAsync<Error, Option<int>>(Some(42));
        var result = await either.SomeToError(Error.New("already exists")).ToEither();

        result.IsLeft.Should().BeTrue();
        result.IfLeft(e => e.Message.Should().Be("already exists"));
    }

    [Fact]
    public async Task SomeToError_WithFunc_SomeValue_ReturnsLeftWithValue()
    {
        var either = RightAsync<Error, Option<string>>(Some("duplicate"));
        var result = await either.SomeToError(v => Error.New($"'{v}' already exists")).ToEither();

        result.IsLeft.Should().BeTrue();
        result.IfLeft(e => e.Message.Should().Be("'duplicate' already exists"));
    }

    [Fact]
    public async Task SomeToError_WithFunc_NoneValue_ReturnsUnit()
    {
        var either = RightAsync<Error, Option<string>>(None);
        var result = await either.SomeToError(v => Error.New($"'{v}' already exists")).ToEither();

        result.IsRight.Should().BeTrue();
    }

    [Fact]
    public async Task SomeToError_LeftValue_PreservesLeft()
    {
        var either = LeftAsync<Error, Option<int>>(Error.New("original error"));
        var result = await either.SomeToError(Error.New("already exists")).ToEither();

        result.IsLeft.Should().BeTrue();
        result.IfLeft(e => e.Message.Should().Be("original error"));
    }
}
