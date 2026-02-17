using System;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

#pragma warning disable CS0618 // Obsolete shim types

/// <summary>
/// C# 14 extension that adds v4 factory methods back to <see cref="Prelude"/>.
/// This allows existing code using <c>Prelude.Try(...)</c>, <c>Prelude.TryAsync(...)</c>,
/// <c>Prelude.Aff(...)</c>, <c>Prelude.RightAsync(...)</c>, and <c>Prelude.LeftAsync(...)</c>
/// to compile without changes.
/// </summary>
public static class PreludeCompat
{
    extension(Prelude)
    {
        /// <summary>
        /// Creates a <c>Try&lt;A&gt;</c> from a function, catching exceptions as <see cref="Error"/>.
        /// Replaces v4's <c>Prelude.Try()</c>.
        /// </summary>
        [Obsolete("Use Try<A> directly or Eff<A> for effectful computations.")]
        public static Try<A> Try<A>(Func<A> f) => new(() =>
        {
            try { return f(); }
            catch (Exception ex) { return Error.New(ex); }
        });

        /// <summary>
        /// Creates a <c>TryAsync&lt;A&gt;</c> from an async function, catching exceptions.
        /// Replaces v4's <c>Prelude.TryAsync()</c>.
        /// </summary>
        [Obsolete("Use Eff<A> for effectful computations.")]
        public static TryAsync<A> TryAsync<A>(Func<Task<A>> f) => new(async () =>
        {
            try { return await f().ConfigureAwait(false); }
            catch (Exception ex) { return Error.New(ex); }
        });

        /// <summary>
        /// Creates a <c>TryAsync&lt;A&gt;</c> from a running task, catching exceptions.
        /// Replaces v4's <c>Prelude.TryAsync(Task)</c>.
        /// </summary>
        [Obsolete("Use Eff<A> for effectful computations.")]
        public static TryAsync<A> TryAsync<A>(Task<A> task) => new(async () =>
        {
            try { return await task.ConfigureAwait(false); }
            catch (Exception ex) { return Error.New(ex); }
        });

        /// <summary>
        /// Creates an <c>Aff&lt;A&gt;</c> from an async function.
        /// Replaces v4's <c>Prelude.Aff()</c>.
        /// </summary>
        [Obsolete("Use Eff<A> for effectful computations.")]
        public static Aff<A> Aff<A>(Func<ValueTask<A>> f) => new(async () =>
        {
            try { return await f().ConfigureAwait(false); }
            catch (Exception ex) { return Error.New(ex); }
        });

        /// <summary>
        /// Creates an <c>EitherAsync</c> in the Right state.
        /// Replaces v4's <c>Prelude.RightAsync()</c>.
        /// </summary>
        [Obsolete("Use Prelude.Right<L, R>() instead.")]
        public static EitherAsync<L, R> RightAsync<L, R>(R value) =>
            EitherAsync<L, R>.Right(value);

        /// <summary>
        /// Creates an <c>EitherAsync</c> in the Left state.
        /// Replaces v4's <c>Prelude.LeftAsync()</c>.
        /// </summary>
        [Obsolete("Use Prelude.Left<L, R>() instead.")]
        public static EitherAsync<L, R> LeftAsync<L, R>(L value) =>
            EitherAsync<L, R>.Left(value);
    }
}

#pragma warning restore CS0618
