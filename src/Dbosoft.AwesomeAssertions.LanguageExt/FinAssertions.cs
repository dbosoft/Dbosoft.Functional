using global::AwesomeAssertions;
using global::AwesomeAssertions.Execution;
using LanguageExt;
using LanguageExt.Common;

namespace Dbosoft.AwesomeAssertions.LanguageExt;

public class FinAssertions<T>(Fin<T> subject, AssertionChain chain)
{
    public Fin<T> Subject { get; } = subject;

    [CustomAssertion]
    public AndWhichConstraint<FinAssertions<T>, T> BeSuccess(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSucc)
            .FailWith("Expected {context:Fin} to be Success{reason}, but found Fail({0}).",
                () => Subject.Match(_ => default(Error)!, e => e));

        var value = Subject.Match(v => v, _ => default!);
        return new AndWhichConstraint<FinAssertions<T>, T>(this, value);
    }

    [CustomAssertion]
    public AndWhichConstraint<FinAssertions<T>, Error> BeFail(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsFail)
            .FailWith("Expected {context:Fin} to be Fail{reason}, but found Success({0}).",
                () => Subject.Match(v => (object?)v, _ => null));

        var error = Subject.Match(_ => default!, e => e);
        return new AndWhichConstraint<FinAssertions<T>, Error>(this, error);
    }
}
