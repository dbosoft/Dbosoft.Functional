using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

#pragma warning disable CS0618 // Obsolete shim types

/// <summary>
/// Extension methods that restore v4-era APIs removed in LanguageExt v5.
/// </summary>
public static class CompatExtensions
{
    // ── Either ↔ EitherAsync conversions ───────────────────────────

    /// <summary>
    /// Convert the Either to an EitherAsync.
    /// </summary>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public static EitherAsync<L, R> ToAsync<L, R>(this Either<L, R> either) =>
        new(Task.FromResult(either));

    /// <summary>
    /// Convert a <c>Task&lt;Either&gt;</c> to an EitherAsync.
    /// </summary>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// Monadic bind on <c>Task&lt;Either&gt;</c> with an async binder.
    /// </summary>
    /// <param name="self">Either to bind</param>
    /// <param name="f">Bind function</param>
    /// <returns>Bound Either</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// Monadic bind on <c>Task&lt;Either&gt;</c> with a synchronous binder.
    /// </summary>
    /// <param name="self">Either to bind</param>
    /// <param name="f">Bind function</param>
    /// <returns>Bound Either</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public static async Task<Either<L, R2>> BindAsync<L, R1, R2>(
        this Task<Either<L, R1>> self,
        Func<R1, Either<L, R2>> f)
    {
        var either = await self.ConfigureAwait(false);
        return either.Bind(f);
    }

    /// <summary>
    /// Maps the value in the Either if it's in a Right state.
    /// </summary>
    /// <param name="self">Either to map</param>
    /// <param name="f">Map function</param>
    /// <returns>Mapped Either</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// </summary>
    /// <param name="self">Either containing an Option</param>
    /// <param name="noneFunc">Function to invoke when None</param>
    /// <returns>Extracted Either</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// Invokes the Some or None function depending on the state of the Option.
    /// </summary>
    /// <param name="option">Option to match</param>
    /// <param name="Some">Function to invoke if in a Some state</param>
    /// <param name="None">Async function to invoke if in a None state</param>
    /// <returns>The return value of the invoked function</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// Convert the structure to an EitherAsync.
    /// </summary>
    /// <param name="option">Option to convert</param>
    /// <param name="left">Default value if the structure is in a None state</param>
    /// <returns>An EitherAsync representation of the structure</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public static EitherAsync<L, A> ToEitherAsync<L, A>(this Option<A> option, L left) =>
        new(Task.FromResult(option.Match(
            Some: a => Prelude.Right<L, A>(a),
            None: () => Prelude.Left<L, A>(left))));

    /// <summary>
    /// Converts an <c>EitherAsync&lt;Error, R&gt;</c> to a <c>Validation&lt;Error, R&gt;</c>.
    /// </summary>
    /// <param name="self">EitherAsync to convert</param>
    /// <returns>A Validation representation of the EitherAsync</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// </summary>
    /// <param name="self">Source sequence</param>
    /// <param name="f">Function to apply to each element</param>
    /// <returns>An EitherAsync containing all Right values or the first Left</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public static EitherAsync<L, IEnumerable<R>> TraverseParallel<A, L, R>(
        this IEnumerable<A> self,
        Func<A, EitherAsync<L, R>> f) =>
        new(RunTraverseParallel(self, f));

    /// <summary>
    /// Runs all <c>EitherAsync</c> computations in parallel and collects results.
    /// Overload accepting <c>Func&lt;A, Task&lt;Either&gt;&gt;</c> directly for better type inference.
    /// </summary>
    /// <param name="self">Source sequence</param>
    /// <param name="f">Async function to apply to each element</param>
    /// <returns>An EitherAsync containing all Right values or the first Left</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
    /// </summary>
    /// <param name="self">Source sequence of EitherAsync</param>
    /// <param name="f">Function to apply to each element</param>
    /// <returns>An EitherAsync containing all Right values or the first Left</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
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
