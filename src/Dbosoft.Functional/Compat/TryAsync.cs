using System;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

#pragma warning disable CS0618 // Obsolete shim types

/// <summary>
/// Compatibility shim for LanguageExt v4's <c>TryAsync</c>.
/// Wraps an async computation that may fail with an exception.
/// </summary>
/// <remarks>
/// This type exists to ease migration from LanguageExt v4 to v5.
/// New code should use <c>Eff&lt;A&gt;</c> for effectful computations.
/// </remarks>
[Obsolete("TryAsync is a v4 compatibility shim. Migrate to Eff<A> for effectful computations.")]
public readonly struct TryAsync<A>
{
    private readonly Func<Task<Fin<A>>> _thunk;

    internal TryAsync(Func<Task<Fin<A>>> thunk) =>
        _thunk = thunk ?? throw new ArgumentNullException(nameof(thunk));

    /// <summary>
    /// Runs the async computation and returns the result.
    /// </summary>
    public Task<Fin<A>> Run() => _thunk();

    /// <summary>
    /// Maps the success value using the specified function.
    /// </summary>
    public TryAsync<B> Map<B>(Func<A, B> f)
    {
        var thunk = _thunk;
        return new(async () =>
        {
            var result = await thunk().ConfigureAwait(false);
            return result.Map(f);
        });
    }

    /// <summary>
    /// Monadically binds the success value.
    /// </summary>
    public TryAsync<B> Bind<B>(Func<A, TryAsync<B>> f)
    {
        var thunk = _thunk;
        return new(async () =>
        {
            var result = await thunk().ConfigureAwait(false);
            return await result.Match(
                Succ: async a => await f(a).Run().ConfigureAwait(false),
                Fail: e => Task.FromResult<Fin<B>>(e)
            ).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Converts to an <c>EitherAsync&lt;Error, A&gt;</c>.
    /// </summary>
    [Obsolete("EitherAsync is a v4 compatibility shim.")]
    public EitherAsync<Error, A> ToEither() =>
        new(ToEitherAsync());

    private async Task<Either<Error, A>> ToEitherAsync()
    {
        var result = await _thunk().ConfigureAwait(false);
        return result.Match(
            Succ: a => Prelude.Right<Error, A>(a),
            Fail: e => Prelude.Left<Error, A>(e)
        );
    }

    /// <summary>
    /// Converts to an <c>EitherAsync&lt;Error, A&gt;</c> with custom error mapping.
    /// </summary>
    [Obsolete("EitherAsync is a v4 compatibility shim.")]
    public EitherAsync<Error, A> ToEither(Func<Exception, Error> mapError) =>
        new(ToEitherAsync(mapError));

    private async Task<Either<Error, A>> ToEitherAsync(Func<Exception, Error> mapError)
    {
        var result = await _thunk().ConfigureAwait(false);
        return result.Match(
            Succ: a => Prelude.Right<Error, A>(a),
            Fail: e => Prelude.Left<Error, A>(mapError(e.ToException()))
        );
    }

    /// <summary>
    /// Pattern matches on the result.
    /// </summary>
    public async Task<B> Match<B>(Func<A, B> Succ, Func<Exception, B> Fail)
    {
        var result = await _thunk().ConfigureAwait(false);
        return result.Match(
            Succ: Succ,
            Fail: e => Fail(e.ToException())
        );
    }

    /// <summary>LINQ Select support.</summary>
    public TryAsync<B> Select<B>(Func<A, B> f) => Map(f);

    /// <summary>LINQ SelectMany support.</summary>
    public TryAsync<B> SelectMany<B>(Func<A, TryAsync<B>> f) => Bind(f);

    /// <summary>LINQ SelectMany with projection support.</summary>
    public TryAsync<C> SelectMany<B, C>(
        Func<A, TryAsync<B>> bind,
        Func<A, B, C> project) =>
        Bind(a => bind(a).Map(b => project(a, b)));

    /// <summary>
    /// Implicit conversion to <c>EitherAsync&lt;Error, A&gt;</c>.
    /// </summary>
    [Obsolete("EitherAsync is a v4 compatibility shim.")]
    public static implicit operator EitherAsync<Error, A>(TryAsync<A> tryAsync) =>
        tryAsync.ToEither();
}

#pragma warning restore CS0618
