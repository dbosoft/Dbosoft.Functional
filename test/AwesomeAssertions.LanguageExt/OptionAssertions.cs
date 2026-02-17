using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using LanguageExt;

namespace AwesomeAssertions.LanguageExt;

public class OptionAssertions<T>(Option<T> subject, AssertionChain chain)
{
    public Option<T> Subject { get; } = subject;

    [CustomAssertion]
    public AndWhichConstraint<OptionAssertions<T>, T> BeSome(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSome)
            .FailWith("Expected {context:Option} to be Some{reason}, but found None.");

        var value = Subject.Match(v => v, () => default!);
        return new AndWhichConstraint<OptionAssertions<T>, T>(this, value);
    }

    [CustomAssertion]
    public AndConstraint<OptionAssertions<T>> BeNone(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsNone)
            .FailWith("Expected {context:Option} to be None{reason}, but found Some.");

        return new AndConstraint<OptionAssertions<T>>(this);
    }

    [CustomAssertion]
    public AndConstraint<OptionAssertions<T>> Be(T expected,
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSome)
            .FailWith("Expected {context:Option} to contain {0}{reason}, but found None.", expected);

        var value = Subject.Match(v => v, () => default!);
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(EqualityComparer<T>.Default.Equals(value, expected))
            .FailWith("Expected {context:Option} to contain {0}{reason}, but found {1}.", expected, value);

        return new AndConstraint<OptionAssertions<T>>(this);
    }
}
