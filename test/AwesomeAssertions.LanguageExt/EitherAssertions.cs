using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using LanguageExt;

namespace AwesomeAssertions.LanguageExt;

public class EitherAssertions<TL, TR>(Either<TL, TR> subject, AssertionChain chain)
{
    public Either<TL, TR> Subject { get; } = subject;

    [CustomAssertion]
    public AndWhichConstraint<EitherAssertions<TL, TR>, TR> BeRight(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsRight)
            .FailWith("Expected {context:Either} to be Right{reason}, but found Left({0}).",
                () => Subject.Match(Left: l => (object?)l, Right: _ => null));

        var value = Subject.Match(Left: _ => default(TR)!, Right: r => r);
        return new AndWhichConstraint<EitherAssertions<TL, TR>, TR>(this, value);
    }

    [CustomAssertion]
    public AndWhichConstraint<EitherAssertions<TL, TR>, TL> BeLeft(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsLeft)
            .FailWith("Expected {context:Either} to be Left{reason}, but found Right({0}).",
                () => Subject.Match(Left: _ => null, Right: r => (object?)r));

        var value = Subject.Match(Left: l => l, Right: _ => default(TL)!);
        return new AndWhichConstraint<EitherAssertions<TL, TR>, TL>(this, value);
    }
}
