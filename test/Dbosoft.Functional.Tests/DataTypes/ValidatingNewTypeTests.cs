using LanguageExt.Common;
using LanguageExt;
using static LanguageExt.Prelude;
using Dbosoft.Functional.DataTypes;

#pragma warning disable CS0618 // Obsolete shim types

namespace Dbosoft.Functional.Tests.DataTypes;

public class ValidatingNewTypeTests
{
    public class TestTypeWithArgumentException
        : ValidatingNewType<TestTypeWithArgumentException, string>
    {
        public TestTypeWithArgumentException(string value) : base(value)
        {
            throw new ArgumentException("The value is invalid", nameof(value));
        }
    }

    public class TestTypeWithValidationException
        : ValidatingNewType<TestTypeWithValidationException, string>
    {
        public TestTypeWithValidationException(string value) : base(value)
        {
            ValidOrThrow(Fail<Error, string>(Error.New("First validation failed"))
                         + Fail<Error, string>(Error.New("Second validation failed")));
        }
    }

    public class TestTypeWithValidationSuccess
        : ValidatingNewType<TestTypeWithValidationSuccess, string>
    {
        public TestTypeWithValidationSuccess(string value) : base(value)
        {
            ValidOrThrow(Success<Error, Unit>(unit));
        }
    }

    [Fact]
    public void New_WithArgumentException_ThrowsArgumentException()
    {
        var act = () => TestTypeWithArgumentException.New("test");
        act.Should().Throw<ArgumentException>()
            .WithMessage("The value is invalid (Parameter 'value')");
    }

    [Fact]
    public void New_WithValidationErrors_ThrowsValidationException()
    {
        var act = () => TestTypeWithValidationException.New("test");
        var exception = act.Should()
            .Throw<TestTypeWithValidationException.ValidationException<TestTypeWithValidationException>>()
            .WithMessage("The value is not a valid TestTypeWithValidationException: "
                + "[First validation failed, Second validation failed]")
            .Subject.Should().ContainSingle().Subject;

        ShouldContainValidationErrors(exception.Errors);
    }

    [Fact]
    public void NewEither_WithArgumentException_ReturnsErrorWithInnerError()
    {
        var result = TestTypeWithArgumentException.NewEither("test");

        var error = result.Should().BeLeft().Which;
        var innerError = error.Inner.Should().BeSome()
            .Which.Should().BeOfType<Exceptional>().Subject;
        innerError.Message.Should().Be("The value is invalid (Parameter 'value')");
        innerError.IsExceptional.Should().BeTrue();
        innerError.Exception.Should().BeSome().Which
            .Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void NewEither_WithValidationErrors_ReturnsErrorWithInnerError()
    {
        var result = TestTypeWithValidationException.NewEither("test");

        var error = result.Should().BeLeft().Which;
        error.Message.Should().Be("The value is not a valid TestTypeWithValidationException.");
        var innerErrors = error.Inner.Should().BeSome().Which
            .Should().BeOfType<ManyErrors>().Subject.Errors;
        ShouldContainValidationErrors(innerErrors);
    }

    [Fact]
    public void NewEither_WithNull_ReturnsError()
    {
        var result = TestTypeWithValidationSuccess.NewEither(null!);

        var error = result.Should().BeLeft().Which;
        error.Message.Should().Be("The value is not a valid TestTypeWithValidationSuccess.");

        var innerError = error.Inner.Should().BeSome().Which;
        innerError.Message.Should().Be("The value cannot be null.");
        innerError.IsExceptional.Should().BeFalse();
        innerError.Exception.Should().BeNone();
    }

    [Fact]
    public void NewValidation_ArgumentException_ReturnsErrorWithException()
    {
        var result = TestTypeWithArgumentException.NewValidation("test");

        var error = result.Should().BeFail().Which;
        error.Message.Should().Be("The value is invalid (Parameter 'value')");
        error.IsExceptional.Should().BeTrue();
        error.Exception.Should().BeSome().Which
            .Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void NewValidation_WithValidationErrors_ReturnsBothErrors()
    {
        var result = TestTypeWithValidationException.NewValidation("test");

        var error = result.Should().BeFail().Which;
        ShouldContainValidationErrors(error);
    }

    [Fact]
    public void NewValidation_WithNull_ReturnsError()
    {
        var result = TestTypeWithValidationSuccess.NewValidation(null!);

        var error = result.Should().BeFail().Which;
        error.Message.Should().Be("The value cannot be null.");
        error.IsExceptional.Should().BeFalse();
        error.Exception.Should().BeNone();
    }

    private static void ShouldContainValidationErrors(Seq<Error> errors)
    {
        ((IEnumerable<Error>)errors).Should().SatisfyRespectively(
            error =>
            {
                error.Message.Should().Be("First validation failed");
                error.IsExceptional.Should().BeFalse();
                error.Exception.Should().BeNone();
            },
            error =>
            {
                error.Message.Should().Be("Second validation failed");
                error.IsExceptional.Should().BeFalse();
                error.Exception.Should().BeNone();
            });
    }

    private static void ShouldContainValidationErrors(Error error)
    {
        var errors = error is ManyErrors many ? many.Errors : Seq1(error);
        ShouldContainValidationErrors(errors);
    }
}
