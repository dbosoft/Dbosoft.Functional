using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

/// <summary>
/// EitherAsync monad — holds one of two values 'Left' or 'Right'.
/// Usually 'Left' is considered 'wrong' or 'in error', and 'Right' is, well, right.
/// So when the Either is in a Left state, it cancels computations like bind or map.
/// You can see Left as an 'early out, with a message'. Unlike Option that has None
/// as its alternative value (i.e. it has an 'early out, but no message').
/// </summary>
/// <remarks>
/// For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.
/// Use this type for imperative-style async APIs that need ergonomic error handling
/// without requiring the full Eff runtime machinery.
/// </remarks>
/// <typeparam name="L">Left</typeparam>
/// <typeparam name="R">Right</typeparam>
public readonly struct EitherAsync<L, R>
{
    private readonly Task<Either<L, R>> _task;

    public EitherAsync(Task<Either<L, R>> task) =>
        _task = task ?? Task.FromResult(default(Either<L, R>));

    /// <summary>
    /// Convert the EitherAsync to an Either.
    /// </summary>
    /// <returns>Either</returns>
    public Task<Either<L, R>> ToEither() => _task;

    /// <summary>
    /// Either constructor. Constructs an Either in a Right state.
    /// </summary>
    /// <param name="value">Right value</param>
    /// <returns>A new EitherAsync instance</returns>
    public static EitherAsync<L, R> Right(R value) =>
        new(Task.FromResult(Prelude.Right<L, R>(value)));

    /// <summary>
    /// Either constructor. Constructs an Either in a Left state.
    /// </summary>
    /// <param name="value">Left value</param>
    /// <returns>A new EitherAsync instance</returns>
    public static EitherAsync<L, R> Left(L value) =>
        new(Task.FromResult(Prelude.Left<L, R>(value)));

    /// <summary>
    /// Maps the value in the Either if it's in a Right state.
    /// </summary>
    /// <typeparam name="R2">Mapped Either type</typeparam>
    /// <param name="f">Map function</param>
    /// <returns>Mapped EitherAsync</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public EitherAsync<L, R2> Map<R2>(Func<R, R2> f) =>
        new(MapAsyncImpl(f));

    /// <summary>
    /// Maps the value in the Either if it's in a Right state.
    /// </summary>
    /// <typeparam name="R2">Mapped Either type</typeparam>
    /// <param name="f">Map function</param>
    /// <returns>Mapped EitherAsync</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public EitherAsync<L, R2> MapAsync<R2>(Func<R, R2> f) => Map(f);

    private async Task<Either<L, R2>> MapAsyncImpl<R2>(Func<R, R2> f)
    {
        var either = await _task.ConfigureAwait(false);
        return either.Map(f);
    }

    /// <summary>
    /// Maps the value in the Either if it's in a Left state.
    /// </summary>
    /// <typeparam name="L2">Mapped Either type</typeparam>
    /// <param name="f">Map function</param>
    /// <returns>Mapped EitherAsync</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public EitherAsync<L2, R> MapLeft<L2>(Func<L, L2> f) =>
        new(MapLeftAsync(f));

    private async Task<Either<L2, R>> MapLeftAsync<L2>(Func<L, L2> f)
    {
        var either = await _task.ConfigureAwait(false);
        return either.MapLeft(f);
    }

    /// <summary>
    /// Monadic bind.
    /// </summary>
    /// <typeparam name="R2">Bound return type</typeparam>
    /// <param name="f">Bind function</param>
    /// <returns>Bound EitherAsync</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public EitherAsync<L, R2> Bind<R2>(Func<R, EitherAsync<L, R2>> f) =>
        new(BindAsync(f));

    private async Task<Either<L, R2>> BindAsync<R2>(Func<R, EitherAsync<L, R2>> f)
    {
        var either = await _task.ConfigureAwait(false);
        return await either.Match(
            Right: async r => await f(r).ToEither().ConfigureAwait(false),
            Left: l => Task.FromResult(Prelude.Left<L, R2>(l))
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Invokes the Right or Left function depending on the state of the Either.
    /// </summary>
    /// <typeparam name="R2">Return type</typeparam>
    /// <param name="Right">Function to invoke if in a Right state</param>
    /// <param name="Left">Function to invoke if in a Left state</param>
    /// <returns>The return value of the invoked function</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public async Task<R2> Match<R2>(Func<R, R2> Right, Func<L, R2> Left)
    {
        var either = await _task.ConfigureAwait(false);
        return either.Match(Right: Right, Left: Left);
    }

    /// <summary>
    /// Converts to an <c>Aff&lt;R&gt;</c> by mapping the Left value to an <see cref="Error"/>.
    /// </summary>
    [Obsolete("Aff is a v4 compatibility shim. Migrate to Eff<R>.")]
    public Aff<R> ToAff(Func<L, Error> mapLeft) =>
        new(ToEffAsync(mapLeft));

    /// <summary>
    /// Convert to an <c>Eff&lt;R&gt;</c> by mapping the Left value to an <see cref="Error"/>.
    /// </summary>
    /// <param name="mapLeft">Function to map the Left value to an Error</param>
    /// <returns>Eff monad</returns>
    public Eff<R> ToEff(Func<L, Error> mapLeft)
    {
        var task = _task;
        return Prelude.liftEff(async () => await task.ConfigureAwait(false))
            .Bind(either => either.Match(
                Right: r => Prelude.SuccessEff(r),
                Left: l => Prelude.FailEff<R>(mapLeft(l))));
    }

    private async ValueTask<Fin<R>> ToEffAsync(Func<L, Error> mapLeft)
    {
        var either = await _task.ConfigureAwait(false);
        return either.Match<Fin<R>>(
            Right: r => r,
            Left: l => mapLeft(l));
    }

    /// <summary>
    /// Invokes the Right or Left action depending on the state of the Either.
    /// </summary>
    /// <param name="RightAsync">Async action to invoke if in a Right state</param>
    /// <param name="Left">Action to invoke if in a Left state</param>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public async Task MatchAsync(Func<R, Task> RightAsync, Action<L> Left)
    {
        var either = await _task.ConfigureAwait(false);
        await either.Match<Task>(
            Right: r => RightAsync(r),
            Left: l => { Left(l); return Task.CompletedTask; }
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Apply — applies a function to this <c>EitherAsync</c> and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="f">Function to apply the applicative to</param>
    /// <returns>Result of applying the function</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public TResult Apply<TResult>(Func<EitherAsync<L, R>, TResult> f) => f(this);

    /// <summary>
    /// Maps the value in the Either if it's in a Right state (LINQ Select support).
    /// </summary>
    /// <typeparam name="R2">Mapped Either type</typeparam>
    /// <param name="f">Map function</param>
    /// <returns>Mapped EitherAsync</returns>
    public EitherAsync<L, R2> Select<R2>(Func<R, R2> f) => Map(f);

    /// <summary>
    /// Monadic bind function (LINQ SelectMany support).
    /// </summary>
    /// <typeparam name="R2">Bound return type</typeparam>
    /// <param name="f">Bind function</param>
    /// <returns>Bound EitherAsync</returns>
    public EitherAsync<L, R2> SelectMany<R2>(Func<R, EitherAsync<L, R2>> f) => Bind(f);

    /// <summary>
    /// Monadic bind function with projection (LINQ SelectMany support).
    /// </summary>
    /// <typeparam name="TInter">Intermediate type</typeparam>
    /// <typeparam name="R2">Projected return type</typeparam>
    /// <param name="bind">Bind function</param>
    /// <param name="project">Projection function</param>
    /// <returns>Bound EitherAsync</returns>
    public EitherAsync<L, R2> SelectMany<TInter, R2>(
        Func<R, EitherAsync<L, TInter>> bind,
        Func<R, TInter, R2> project) =>
        Bind(r => bind(r).Map(t => project(r, t)));

    /// <summary>
    /// Custom awaiter that turns an EitherAsync into an Either.
    /// </summary>
    public TaskAwaiter<Either<L, R>> GetAwaiter() => _task.GetAwaiter();

    /// <summary>
    /// Implicit conversion operator from L to EitherAsync L R.
    /// </summary>
    /// <param name="left">Left value</param>
    public static implicit operator EitherAsync<L, R>(L left) =>
        Left(left);

    /// <summary>
    /// Implicit conversion from a synchronous Either to EitherAsync.
    /// </summary>
    /// <param name="either">Either value</param>
    public static implicit operator EitherAsync<L, R>(Either<L, R> either) =>
        new(Task.FromResult(either));

    /// <summary>
    /// Implicit conversion from a <c>Task&lt;Either&lt;L, R&gt;&gt;</c> to EitherAsync.
    /// </summary>
    /// <param name="task">Task of Either</param>
    public static implicit operator EitherAsync<L, R>(Task<Either<L, R>> task) =>
        new(task);

    /// <summary>
    /// Implicit conversion to a <c>Task&lt;Either&lt;L, R&gt;&gt;</c>.
    /// Allows returning EitherAsync from methods that return <c>Task&lt;Either&gt;</c>.
    /// </summary>
    /// <param name="ea">EitherAsync value</param>
    public static implicit operator Task<Either<L, R>>(EitherAsync<L, R> ea) =>
        ea.ToEither();
}
