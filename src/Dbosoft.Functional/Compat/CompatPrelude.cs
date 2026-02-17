using System;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

#pragma warning disable CS0618 // Obsolete shim types

/// <summary>
/// Prelude functions for <c>EitherAsync</c> and v4 compatibility shims.
/// Add <c>using static LanguageExt.CompatPrelude;</c> as an alternative to the
/// C# 14 <see cref="PreludeCompat"/> extension on <see cref="Prelude"/>.
/// </summary>
public static class CompatPrelude
{
    /// <summary>
    /// Either constructor. Constructs an Either in a Right state.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Right value</param>
    /// <returns>A new EitherAsync instance</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public static EitherAsync<L, R> RightAsync<L, R>(R value) =>
        EitherAsync<L, R>.Right(value);

    /// <summary>
    /// Either constructor. Constructs an Either in a Left state.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Left value</param>
    /// <returns>A new EitherAsync instance</returns>
    /// <remarks>For new code, prefer <c>Eff&lt;R&gt;</c> or <c>Eff&lt;RT, R&gt;</c>.</remarks>
    public static EitherAsync<L, R> LeftAsync<L, R>(L value) =>
        EitherAsync<L, R>.Left(value);

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
}

#pragma warning restore CS0618
