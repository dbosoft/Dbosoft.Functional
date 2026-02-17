using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

/// <summary>
/// Compatibility shim for LanguageExt v4's <c>EitherAsync</c>.
/// Wraps a <see cref="Task{T}"/> of <see cref="Either{L,R}"/> to provide the v4 API surface.
/// </summary>
/// <remarks>
/// This type exists to ease migration from LanguageExt v4 to v5.
/// New code should use <c>Either&lt;L, R&gt;</c> with async/await,
/// or <c>Eff&lt;R&gt;</c> for effectful computations.
/// </remarks>
[Obsolete("EitherAsync is a v4 compatibility shim. Migrate to Either<L, R> with async/await or Eff<R>.")]
public readonly struct EitherAsync<L, R>
{
    private readonly Task<Either<L, R>> _task;

    public EitherAsync(Task<Either<L, R>> task) =>
        _task = task ?? Task.FromResult(default(Either<L, R>));

    /// <summary>
    /// Extracts the inner <c>Task&lt;Either&lt;L, R&gt;&gt;</c>.
    /// </summary>
    public Task<Either<L, R>> ToEither() => _task;

    /// <summary>
    /// Creates an <c>EitherAsync</c> in the Right state.
    /// </summary>
    public static EitherAsync<L, R> Right(R value) =>
        new(Task.FromResult(Prelude.Right<L, R>(value)));

    /// <summary>
    /// Creates an <c>EitherAsync</c> in the Left state.
    /// </summary>
    public static EitherAsync<L, R> Left(L value) =>
        new(Task.FromResult(Prelude.Left<L, R>(value)));

    /// <summary>
    /// Maps the Right value using the specified function.
    /// </summary>
    public EitherAsync<L, R2> Map<R2>(Func<R, R2> f) =>
        new(MapAsyncImpl(f));

    /// <summary>
    /// Maps the Right value using the specified function.
    /// Alias for <see cref="Map{R2}"/> for v4 compatibility.
    /// </summary>
    public EitherAsync<L, R2> MapAsync<R2>(Func<R, R2> f) => Map(f);

    private async Task<Either<L, R2>> MapAsyncImpl<R2>(Func<R, R2> f)
    {
        var either = await _task.ConfigureAwait(false);
        return either.Map(f);
    }

    /// <summary>
    /// Maps the Left value using the specified function.
    /// </summary>
    public EitherAsync<L2, R> MapLeft<L2>(Func<L, L2> f) =>
        new(MapLeftAsync(f));

    private async Task<Either<L2, R>> MapLeftAsync<L2>(Func<L, L2> f)
    {
        var either = await _task.ConfigureAwait(false);
        return either.MapLeft(f);
    }

    /// <summary>
    /// Monadically binds the Right value using the specified function.
    /// </summary>
    public EitherAsync<L, R2> Bind<R2>(Func<R, EitherAsync<L, R2>> f) =>
        new(BindAsync(f));

    #pragma warning disable CS0618 // Obsolete
    private async Task<Either<L, R2>> BindAsync<R2>(Func<R, EitherAsync<L, R2>> f)
    {
        var either = await _task.ConfigureAwait(false);
        return await either.Match(
            Right: async r => await f(r).ToEither().ConfigureAwait(false),
            Left: l => Task.FromResult(Prelude.Left<L, R2>(l))
        ).ConfigureAwait(false);
    }
    #pragma warning restore CS0618

    /// <summary>
    /// Pattern matches on the Either state.
    /// </summary>
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
        new(ToAffAsync(mapLeft));

    private async ValueTask<Fin<R>> ToAffAsync(Func<L, Error> mapLeft)
    {
        var either = await _task.ConfigureAwait(false);
        return either.Match<Fin<R>>(
            Right: r => r,
            Left: l => mapLeft(l));
    }

    /// <summary>
    /// Async pattern match with async Right handler and sync Left handler.
    /// Replaces v4's <c>EitherAsync.MatchAsync</c>.
    /// </summary>
    public async Task MatchAsync(Func<R, Task> RightAsync, Action<L> Left)
    {
        var either = await _task.ConfigureAwait(false);
        await either.Match<Task>(
            Right: r => RightAsync(r),
            Left: l => { Left(l); return Task.CompletedTask; }
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Pipe-through: applies a function to this <c>EitherAsync</c> and returns the result.
    /// Replaces v4's <c>Apply</c> pattern used for error handling middleware.
    /// </summary>
    public TResult Apply<TResult>(Func<EitherAsync<L, R>, TResult> f) => f(this);

    /// <summary>
    /// LINQ Select support.
    /// </summary>
    public EitherAsync<L, R2> Select<R2>(Func<R, R2> f) => Map(f);

    /// <summary>
    /// LINQ SelectMany support.
    /// </summary>
    public EitherAsync<L, R2> SelectMany<R2>(Func<R, EitherAsync<L, R2>> f) => Bind(f);

    /// <summary>
    /// LINQ SelectMany with projection support.
    /// </summary>
    public EitherAsync<L, R2> SelectMany<TInter, R2>(
        Func<R, EitherAsync<L, TInter>> bind,
        Func<R, TInter, R2> project) =>
        Bind(r => bind(r).Map(t => project(r, t)));

    /// <summary>
    /// Makes <c>EitherAsync</c> awaitable. Returns <c>Either&lt;L, R&gt;</c>.
    /// </summary>
    public TaskAwaiter<Either<L, R>> GetAwaiter() => _task.GetAwaiter();

    /// <summary>
    /// Implicit conversion from a synchronous <c>Either</c>.
    /// </summary>
    public static implicit operator EitherAsync<L, R>(Either<L, R> either) =>
        new(Task.FromResult(either));

    /// <summary>
    /// Implicit conversion from a <c>Task&lt;Either&lt;L, R&gt;&gt;</c>.
    /// </summary>
    public static implicit operator EitherAsync<L, R>(Task<Either<L, R>> task) =>
        new(task);

    /// <summary>
    /// Implicit conversion to a <c>Task&lt;Either&lt;L, R&gt;&gt;</c>.
    /// Allows returning <c>EitherAsync</c> from methods that return <c>Task&lt;Either&gt;</c>.
    /// </summary>
    public static implicit operator Task<Either<L, R>>(EitherAsync<L, R> ea) =>
        ea.ToEither();
}
