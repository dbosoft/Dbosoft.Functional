using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using static LanguageExt.CompatPrelude;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace LanguageExt
{
    #pragma warning disable CS0618 // Obsolete shim types

    public static class EitherExtensions
    {

        public static Task<Either<Error, TIn>> ToEitherRight<TIn>(this TIn right)
        {
            return RightAsync<Error, TIn>(right).ToEither();
        }

        public static Task<Either<Error, TIn>> ToEitherLeft<TIn>(this Error error)
        {
            return LeftAsync<Error, TIn>(error).ToEither();
        }

        public static Task<Either<Error, TIn>> IfNoneAsync<TIn>(this Task<Either<Error, Option<TIn>>> either,
            Func<Task<Either<Error, TIn>>> noneFunc)
        {
            return either.ToAsync().Bind(r => r.MatchAsync(
                    s => Prelude.Right<Error, TIn>(s),
                    None: noneFunc).ToAsync()
            ).ToEither();
        }

        /// <summary>
        /// Throws the error as an exception if the Either is in a Left state,
        /// otherwise returns the Right value.
        /// </summary>
        public static TRight ThrowIfLeft<TRight>(this Either<Error, TRight> either)
        {
            return either.Match(
                Right: r => r,
                Left: e => e.ToException().Rethrow<TRight>());
        }

        /// <summary>
        /// Converts an <see cref="EitherAsync{Error, Option}"/> to an <see cref="EitherAsync{Error, T}"/>
        /// by replacing <c>None</c> with the specified error.
        /// </summary>
        public static EitherAsync<Error, T> NoneToError<T>(
            this EitherAsync<Error, Option<T>> either, Error error)
        {
            return either.Bind(o => o.ToEither(error).ToAsync());
        }

        /// <summary>
        /// Converts an <see cref="EitherAsync{Error, Option}"/> to an <see cref="EitherAsync{Error, Unit}"/>
        /// by replacing <c>Some</c> with the specified error. Useful for existence checks.
        /// </summary>
        public static EitherAsync<Error, Unit> SomeToError<T>(
            this EitherAsync<Error, Option<T>> either, Error error)
        {
            return either.Bind(o => o.Match(
                Some: _ => EitherAsync<Error, Unit>.Left(error),
                None: () => EitherAsync<Error, Unit>.Right(Prelude.unit)));
        }

        /// <summary>
        /// Converts an <see cref="EitherAsync{Error, Option}"/> to an <see cref="EitherAsync{Error, Unit}"/>
        /// by replacing <c>Some</c> with an error derived from the value. Useful for existence checks.
        /// </summary>
        public static EitherAsync<Error, Unit> SomeToError<T>(
            this EitherAsync<Error, Option<T>> either, Func<T, Error> errorFunc)
        {
            return either.Bind(o => o.Match(
                Some: v => EitherAsync<Error, Unit>.Left(errorFunc(v)),
                None: () => EitherAsync<Error, Unit>.Right(Prelude.unit)));
        }
    }

    #pragma warning restore CS0618
}
