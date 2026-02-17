using System.Threading.Tasks;
using LanguageExt.Common;

using static LanguageExt.Prelude;

// ReSharper disable once CheckNamespace
namespace LanguageExt
{
    #pragma warning disable CS0618 // Obsolete shim types

    public static class AffExtensions
    {
        /// <summary>
        /// Converts an <see cref="EitherAsync{Error, R}"/> to an <see cref="Aff{R}"/>.
        /// Shorthand for <c>either.ToAff(identity)</c>.
        /// </summary>
        public static Aff<R> ToAff<R>(this EitherAsync<Error, R> either) =>
            either.ToAff(identity);

        /// <summary>
        /// Converts a <see cref="ValueTask{Fin}"/> to an <see cref="EitherAsync{Error, R}"/>.
        /// </summary>
        public static EitherAsync<Error, R> ToEitherAsync<R>(this ValueTask<Fin<R>> fin) =>
            new(fin.AsTask().Map(f => f.ToEither()));
    }

    #pragma warning restore CS0618
}
