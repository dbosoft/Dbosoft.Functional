using System;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt;

/// <summary>
/// Compatibility shim for LanguageExt v4's <c>Aff</c>.
/// Wraps an async effect that produces a <see cref="Fin{R}"/>.
/// </summary>
/// <remarks>
/// This type exists to ease migration from LanguageExt v4 to v5.
/// New code should use <c>Eff&lt;R&gt;</c> which handles both sync and async.
/// </remarks>
[Obsolete("Aff is a v4 compatibility shim. Migrate to Eff<R>.")]
public readonly struct Aff<R>
{
    private readonly Func<ValueTask<Fin<R>>> _thunk;

    internal Aff(Func<ValueTask<Fin<R>>> thunk) =>
        _thunk = thunk ?? throw new ArgumentNullException(nameof(thunk));

    internal Aff(ValueTask<Fin<R>> task) =>
        _thunk = () => task;

    /// <summary>
    /// Runs the effect and returns the result.
    /// </summary>
    public ValueTask<Fin<R>> Run() => _thunk();

    /// <summary>
    /// Creates an <c>Aff</c> that immediately fails with the specified error message.
    /// Replaces v4's <c>Aff&lt;R&gt;.Fail(string)</c>.
    /// </summary>
    public static Aff<R> Fail(string message) =>
        new(() => new ValueTask<Fin<R>>(Error.New(message)));

    /// <summary>
    /// Creates an <c>Aff</c> that immediately fails with the specified error.
    /// Replaces v4's <c>Aff&lt;R&gt;.Fail(Error)</c>.
    /// </summary>
    public static Aff<R> Fail(Error error) =>
        new(() => new ValueTask<Fin<R>>(error));
}
