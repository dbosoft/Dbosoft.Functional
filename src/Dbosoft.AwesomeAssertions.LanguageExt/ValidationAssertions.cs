using global::AwesomeAssertions;
using global::AwesomeAssertions.Execution;
using LanguageExt;
using LanguageExt.Traits;

namespace Dbosoft.AwesomeAssertions.LanguageExt;

public class ValidationAssertions<F, A>(Validation<F, A> subject, AssertionChain chain)
    where F : Monoid<F>
{
    public Validation<F, A> Subject { get; } = subject;

    [CustomAssertion]
    public AndConstraint<ValidationAssertions<F, A>> BeSuccess(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSuccess)
            .FailWith("Expected {context:Validation} to be Success{reason}, but found Fail.");

        return new AndConstraint<ValidationAssertions<F, A>>(this);
    }

    [CustomAssertion]
    public AndWhichConstraint<ValidationAssertions<F, A>, F> BeFail(
        string because = "", params object[] becauseArgs)
    {
        chain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsFail)
            .FailWith("Expected {context:Validation} to be Fail{reason}, but found Success.");

        var failValue = Subject.Match(Succ: _ => default(F)!, Fail: f => f);
        return new AndWhichConstraint<ValidationAssertions<F, A>, F>(this, failValue);
    }
}
