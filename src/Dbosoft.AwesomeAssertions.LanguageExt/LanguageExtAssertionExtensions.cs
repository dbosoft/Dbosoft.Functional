using global::AwesomeAssertions.Execution;

using LanguageExt;
using LanguageExt.Traits;

namespace Dbosoft.AwesomeAssertions.LanguageExt;

public static class LanguageExtAssertionExtensions
{
    public static OptionAssertions<T> Should<T>(this Option<T> instance)
        => new(instance, AssertionChain.GetOrCreate());

    public static EitherAssertions<TL, TR> Should<TL, TR>(this Either<TL, TR> instance)
        => new(instance, AssertionChain.GetOrCreate());

    public static FinAssertions<T> Should<T>(this Fin<T> instance)
        => new(instance, AssertionChain.GetOrCreate());

    public static ValidationAssertions<F, A> Should<F, A>(this Validation<F, A> instance)
        where F : Monoid<F>
        => new(instance, AssertionChain.GetOrCreate());
}
