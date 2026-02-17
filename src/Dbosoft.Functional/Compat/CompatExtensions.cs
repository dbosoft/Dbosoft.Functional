using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

#pragma warning disable CS0618 // Obsolete shim types

/// <summary>
/// Extension methods that restore v4-era APIs removed in LanguageExt v5.
/// These exist for migration compatibility and should be replaced with
/// native v5 patterns over time.
/// </summary>
[Obsolete("These extensions are v4 compatibility shims. Migrate to native v5 patterns.")]
public static class CompatExtensions
{
    // ── Either ↔ EitherAsync conversions ───────────────────────────

    /// <summary>
    /// Wraps a synchronous <c>Either</c> in an <c>EitherAsync</c>.
    /// Replaces v4's <c>Either.ToAsync()</c>.
    /// </summary>
    [Obsolete("EitherAsync is a v4 compatibility shim. Use Either<L, R> with async/await instead.")]
    public static EitherAsync<L, R> ToAsync<L, R>(this Either<L, R> either) =>
        new(Task.FromResult(either));

    /// <summary>
    /// Wraps a <c>Task&lt;Either&gt;</c> in an <c>EitherAsync</c>.
    /// Replaces v4's <c>Task&lt;Either&gt;.ToAsync()</c>.
    /// </summary>
    [Obsolete("EitherAsync is a v4 compatibility shim. Use Task<Either<L, R>> with async/await instead.")]
    public static EitherAsync<L, R> ToAsync<L, R>(this Task<Either<L, R>> task) =>
        new(task);

    // ── Try ↔ TryAsync / Either conversions ────────────────────────

    /// <summary>
    /// Converts a <c>Try&lt;A&gt;</c> to a <c>TryAsync&lt;A&gt;</c>.
    /// Replaces v4's <c>Try.ToAsync()</c>.
    /// </summary>
    [Obsolete("TryAsync is a v4 compatibility shim. Use Eff<A> instead.")]
    public static TryAsync<A> ToAsync<A>(this Try<A> @try) =>
        new(() =>
        {
            try { return Task.FromResult(@try.Run()); }
            catch (Exception ex) { return Task.FromResult<Fin<A>>(Error.New(ex)); }
        });

    /// <summary>
    /// Converts a <c>Try&lt;A&gt;</c> to an <c>Either&lt;L, A&gt;</c> with custom error mapping.
    /// Replaces v4's <c>Try.ToEither(Func&lt;Exception, L&gt;)</c>.
    /// </summary>
    [Obsolete("Use Try<A>.Run() and match the Fin directly.")]
    public static Either<L, A> ToEither<L, A>(this Try<A> @try, Func<Exception, L> mapLeft)
    {
        var fin = @try.Run();
        return fin.Match(
            Succ: a => Prelude.Right<L, A>(a),
            Fail: e => Prelude.Left<L, A>(mapLeft(e.ToException()))
        );
    }

    // ── Task<Either> bind / map extensions ─────────────────────────

    /// <summary>
    /// Async bind on <c>Task&lt;Either&gt;</c> with an async binder.
    /// Replaces v4's transformer-based <c>BindAsync</c>.
    /// </summary>
    [Obsolete("Use async/await with Either<L, R> instead.")]
    public static async Task<Either<L, R2>> BindAsync<L, R1, R2>(
        this Task<Either<L, R1>> self,
        Func<R1, Task<Either<L, R2>>> f)
    {
        var either = await self.ConfigureAwait(false);
        return await either.Match(
            Right: r => f(r),
            Left: l => Task.FromResult(Prelude.Left<L, R2>(l))
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Sync bind on <c>Task&lt;Either&gt;</c> with a synchronous binder.
    /// Replaces v4's transformer-based <c>BindAsync</c> when the binder returns a sync Either.
    /// </summary>
    [Obsolete("Use async/await with Either<L, R> instead.")]
    public static async Task<Either<L, R2>> BindAsync<L, R1, R2>(
        this Task<Either<L, R1>> self,
        Func<R1, Either<L, R2>> f)
    {
        var either = await self.ConfigureAwait(false);
        return either.Bind(f);
    }

    /// <summary>
    /// Synchronous map on the Right value of <c>Task&lt;Either&gt;</c>.
    /// Replaces v4's <c>EitherAsync.MapAsync</c> which mapped the Right value.
    /// </summary>
    [Obsolete("Use async/await with Either<L, R> instead.")]
    public static async Task<Either<L, R2>> MapAsync<L, R, R2>(
        this Task<Either<L, R>> self,
        Func<R, R2> f)
    {
        var either = await self.ConfigureAwait(false);
        return either.Map(f);
    }

    /// <summary>
    /// Extracts from <c>Task&lt;Either&lt;L, Option&lt;A&gt;&gt;&gt;</c>.
    /// If Right(Some(a)) returns Right(a), if Right(None) calls noneFunc, if Left propagates.
    /// Replaces v4's <c>IfNoneAsync</c> transformer.
    /// </summary>
    [Obsolete("Use async/await with Either and Option instead.")]
    public static async Task<Either<L, A>> IfNoneAsync<L, A>(
        this Task<Either<L, Option<A>>> self,
        Func<Task<Either<L, A>>> noneFunc)
    {
        var either = await self.ConfigureAwait(false);
        return await either.Match(
            Right: async opt => await opt.Match(
                Some: a => Task.FromResult(Prelude.Right<L, A>(a)),
                None: noneFunc
            ).ConfigureAwait(false),
            Left: l => Task.FromResult(Prelude.Left<L, A>(l))
        ).ConfigureAwait(false);
    }

    // ── Option extensions ──────────────────────────────────────────

    /// <summary>
    /// Async match on <c>Option</c> with mixed sync/async handlers.
    /// Replaces v4's <c>Option.MatchAsync()</c>.
    /// </summary>
    [Obsolete("Use Option<A>.Match() with async/await instead.")]
    public static async Task<B> MatchAsync<A, B>(
        this Option<A> option,
        Func<A, B> Some,
        Func<Task<B>> None)
    {
        return await option.Match(
            Some: s => Task.FromResult(Some(s)),
            None: None
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Flattens an <c>Option&lt;IEnumerable&lt;T&gt;&gt;</c> to <c>IEnumerable&lt;T&gt;</c>.
    /// Returns the inner sequence if Some, or empty if None.
    /// Replaces v4's <c>Option.Flatten()</c>.
    /// </summary>
    [Obsolete("Use Option.Match() with Enumerable.Empty instead.")]
    public static IEnumerable<T> Flatten<T>(this Option<IEnumerable<T>> self) =>
        self.Match(
            Some: x => x,
            None: Enumerable.Empty<T>);

    /// <summary>
    /// Converts an <c>Option&lt;A&gt;</c> to an <c>EitherAsync&lt;L, A&gt;</c>,
    /// using the provided value for the Left case when None.
    /// Replaces v4's <c>Option.ToEitherAsync(L)</c>.
    /// </summary>
    [Obsolete("Use Option.ToEither() with async/await instead.")]
    public static EitherAsync<L, A> ToEitherAsync<L, A>(this Option<A> option, L left) =>
        new(Task.FromResult(option.Match(
            Some: a => Prelude.Right<L, A>(a),
            None: () => Prelude.Left<L, A>(left))));

    /// <summary>
    /// Converts an <c>EitherAsync&lt;Error, R&gt;</c> to a <c>Validation&lt;Error, R&gt;</c>.
    /// Replaces v4's <c>EitherAsync.ToValidation()</c>.
    /// </summary>
    [Obsolete("Use Either with async/await instead of EitherAsync.")]
    public static async Task<Validation<Error, R>> ToValidation<R>(this EitherAsync<Error, R> self)
    {
        var either = await self.ToEither().ConfigureAwait(false);
        return either.Match(
            Right: r => Prelude.Success<Error, R>(r),
            Left: l => Prelude.Fail<Error, R>(l));
    }

    // ── Error extensions ───────────────────────────────────────────

    /// <summary>
    /// Checks if the <see cref="Error"/>'s inner exception is of type <typeparamref name="T"/>.
    /// Replaces v4's generic <c>Error.Is&lt;T&gt;()</c> which was removed in v5.
    /// </summary>
    [Obsolete("Check Error.Exception directly instead.")]
    public static bool Is<T>(this Error error) where T : Exception =>
        error.Exception.Match(e => e is T, () => false);

    // ── IEnumerable extensions ─────────────────────────────────────

    /// <summary>
    /// Maps each element of an <c>IEnumerable</c> using the specified function.
    /// Replaces v4's <c>IEnumerable.Map()</c> extension which was removed in v5.
    /// </summary>
    [Obsolete("Use LINQ Select instead.")]
    public static IEnumerable<U> Map<T, U>(this IEnumerable<T> self, Func<T, U> f) =>
        self.Select(f);

    /// <summary>
    /// Applies an action to each element of an <c>IEnumerable</c>.
    /// Replaces v4's <c>IEnumerable.Iter()</c> extension.
    /// </summary>
    [Obsolete("Use foreach instead.")]
    public static Unit Iter<T>(this IEnumerable<T> self, Action<T> action)
    {
        foreach (var item in self) action(item);
        return Unit.Default;
    }

    /// <summary>
    /// Concatenates two sequences.
    /// Replaces v4's <c>IEnumerable.Append(IEnumerable)</c> extension which was removed in v5.
    /// </summary>
    [Obsolete("Use LINQ Concat instead.")]
    public static IEnumerable<T> Append<T>(this IEnumerable<T> self, IEnumerable<T> other) =>
        self.Concat(other);

    /// <summary>
    /// Runs all <c>EitherAsync</c> computations in parallel and collects results.
    /// Returns the first Left encountered, or all Right values.
    /// Replaces v4's <c>TraverseParallel</c>.
    /// </summary>
    [Obsolete("Use Task.WhenAll with Either instead.")]
    public static EitherAsync<L, IEnumerable<R>> TraverseParallel<A, L, R>(
        this IEnumerable<A> self,
        Func<A, EitherAsync<L, R>> f) =>
        new(RunTraverseParallel(self, f));

    /// <summary>
    /// Overload accepting <c>Func&lt;A, Task&lt;Either&gt;&gt;</c> directly for better type inference.
    /// </summary>
    [Obsolete("Use Task.WhenAll with Either instead.")]
    public static EitherAsync<L, IEnumerable<R>> TraverseParallel<A, L, R>(
        this IEnumerable<A> self,
        Func<A, Task<Either<L, R>>> f) =>
        new(RunTraverseParallel(self, a => new EitherAsync<L, R>(f(a))));

    private static async Task<Either<L, IEnumerable<R>>> RunTraverseParallel<A, L, R>(
        IEnumerable<A> source,
        Func<A, EitherAsync<L, R>> f)
    {
        var tasks = source.Select(a => f(a).ToEither()).ToArray();
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var rights = new List<R>(results.Length);
        foreach (var result in results)
        {
            if (result.IsLeft)
                return result.Match(
                    Right: _ => default!,
                    Left: l => Prelude.Left<L, IEnumerable<R>>(l));
            rights.Add(result.Match(
                Right: r => r,
                Left: _ => default!));
        }
        return Prelude.Right<L, IEnumerable<R>>(rights.AsEnumerable());
    }

    // ── Task extensions ────────────────────────────────────────────
    // Note: Task<A>.Map(Func<A,B>) is provided by LanguageExt v5's TaskExtensions.

    /// <summary>
    /// Converts a <c>Task</c> to <c>Task&lt;Unit&gt;</c>.
    /// Replaces v4's <c>Task.ToUnit()</c> extension.
    /// </summary>
    [Obsolete("Use async/await instead.")]
    public static async Task<Unit> ToUnit(this Task self)
    {
        await self.ConfigureAwait(false);
        return Unit.Default;
    }

    /// <summary>
    /// Converts a <c>Task&lt;T&gt;</c> to <c>Task&lt;Unit&gt;</c>, discarding the result.
    /// Replaces v4's <c>Task.ToUnit()</c> extension.
    /// </summary>
    [Obsolete("Use async/await instead.")]
    public static async Task<Unit> ToUnit<T>(this Task<T> self)
    {
        await self.ConfigureAwait(false);
        return Unit.Default;
    }

    // ── Either extensions ──────────────────────────────────────────

    /// <summary>
    /// Returns a single-element enumerable of the Right value, or empty if Left.
    /// Replaces v4's <c>Either.RightAsEnumerable()</c>.
    /// </summary>
    [Obsolete("Use Either.Match() instead.")]
    public static IEnumerable<R> RightAsEnumerable<L, R>(this Either<L, R> either) =>
        either.Match(
            Right: r => new[] { r },
            Left: _ => Array.Empty<R>());

    /// <summary>
    /// Runs all <c>EitherAsync</c> computations sequentially and collects results.
    /// Replaces v4's <c>Traverse</c> on <c>IEnumerable&lt;EitherAsync&gt;</c>.
    /// </summary>
    [Obsolete("Use async/await with Task.WhenAll instead.")]
    public static EitherAsync<L, IEnumerable<R>> Traverse<L, R>(
        this IEnumerable<EitherAsync<L, R>> self,
        Func<EitherAsync<L, R>, EitherAsync<L, R>> f) =>
        new(RunTraverse(self, f));

    private static async Task<Either<L, IEnumerable<R>>> RunTraverse<L, R>(
        IEnumerable<EitherAsync<L, R>> source,
        Func<EitherAsync<L, R>, EitherAsync<L, R>> f)
    {
        var rights = new List<R>();
        foreach (var item in source)
        {
            var result = await f(item).ToEither().ConfigureAwait(false);
            if (result.IsLeft)
                return result.Match(
                    Right: _ => default!,
                    Left: l => Prelude.Left<L, IEnumerable<R>>(l));
            rights.Add(result.Match(
                Right: r => r,
                Left: _ => default!));
        }
        return Prelude.Right<L, IEnumerable<R>>(rights.AsEnumerable());
    }

    /// <summary>
    /// Traverses an <c>IEnumerable</c> with an Option-returning function.
    /// Returns <c>Some(results)</c> if all elements produce Some, or <c>None</c> if any is None.
    /// Replaces v4's <c>IEnumerable.Traverse(Func&lt;A, Option&lt;B&gt;&gt;)</c>.
    /// </summary>
    [Obsolete("Use LINQ and Option matching instead.")]
    public static Option<IEnumerable<B>> Traverse<A, B>(
        this IEnumerable<A> self,
        Func<A, Option<B>> f)
    {
        var results = new List<B>();
        foreach (var item in self)
        {
            var opt = f(item);
            if (opt.IsNone)
                return Prelude.None;
            results.Add(opt.Match(
                Some: x => x,
                None: () => default!));
        }
        return Prelude.Some(results.AsEnumerable());
    }
}

#pragma warning restore CS0618
