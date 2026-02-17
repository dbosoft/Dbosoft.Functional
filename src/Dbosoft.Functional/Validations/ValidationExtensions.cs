using LanguageExt;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace Dbosoft.Functional.Validations;

/// <summary>
/// Extension methods for <see cref="Validation{F, A}"/> to provide
/// v4-compatible conjunction semantics.
/// </summary>
/// <remarks>
/// In LanguageExt v4, the <c>|</c> operator on <c>Validation</c> had conjunction
/// semantics (both must succeed, errors are accumulated). In v5, <c>|</c> is
/// disjunction (first success wins). Use <see cref="CombineAll{F, A}"/> to get
/// the v4 behavior.
/// </remarks>
public static class ValidationExtensions
{
    /// <summary>
    /// Combines two validations such that both must succeed.
    /// If both fail, errors are accumulated using the <see cref="Monoid{A}"/> instance.
    /// If both succeed, the first success value is returned.
    /// </summary>
    /// <remarks>
    /// This method provides the same semantics as the v4 <c>|</c> operator.
    /// </remarks>
    public static Validation<F, A> CombineAll<F, A>(
        this Validation<F, A> lhs, Validation<F, A> rhs)
        where F : Monoid<F> =>
        lhs.Match(
            Succ: _ => rhs.Match(
                Succ: _ => lhs,
                Fail: _ => rhs),
            Fail: lf => rhs.Match(
                Succ: _ => lhs,
                Fail: rf => Fail<F, A>(lf + rf)));
}
