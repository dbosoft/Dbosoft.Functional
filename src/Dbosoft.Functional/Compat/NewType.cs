using System;
using LanguageExt.ClassInstances;
using LanguageExt.ClassInstances.Pred;
using LanguageExt.TypeClasses;

#nullable enable

namespace LanguageExt;

#pragma warning disable CS0618 // Obsolete shim types

/// <summary>
/// Result of <see cref="NewType{NEWTYPE,A,PRED,ORD}.NewTry"/>.
/// Provides a <c>Match</c> method with Succ/Fail handlers.
/// </summary>
public readonly struct NewTryResult<A>
{
    private readonly A _value;
    private readonly Exception? _exception;
    private readonly bool _isSucc;

    private NewTryResult(A value, Exception? exception, bool isSucc)
    {
        _value = value;
        _exception = exception;
        _isSucc = isSucc;
    }

    internal static NewTryResult<A> Succ(A value) => new(value, null, true);
    internal static NewTryResult<A> Fail(Exception ex) => new(default!, ex, false);

    public R Match<R>(Func<A, R> Succ, Func<Exception, R> Fail) =>
        _isSucc ? Succ(_value) : Fail(_exception!);
}

/// <summary>
/// v4 compatibility shim for <c>NewType&lt;NEWTYPE, A, PRED, ORD&gt;</c>.
/// Provides a value wrapper with validation, equality, and comparison.
/// </summary>
/// <remarks>
/// This type exists to ease migration from LanguageExt v4 to v5.
/// New code should use <c>DomainType&lt;SELF, REPR&gt;</c> or plain C# record types.
/// </remarks>
[Obsolete("NewType is a v4 compatibility shim. Migrate to DomainType<SELF, REPR> or record types.")]
public abstract class NewType<NEWTYPE, A, PRED, ORD> : IEquatable<NEWTYPE>, IComparable<NEWTYPE>
    where NEWTYPE : NewType<NEWTYPE, A, PRED, ORD>
    where PRED : struct, Pred<A>
    where ORD : struct, Ord<A>
{
    public A Value { get; }

    protected NewType(A value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        if (!default(PRED).True(value))
            throw new ArgumentException("Predicate validation failed.", nameof(value));
        Value = value;
    }

    public static NEWTYPE New(A value)
    {
        try
        {
            return (NEWTYPE)Activator.CreateInstance(typeof(NEWTYPE), value)!;
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // unreachable but required by compiler
        }
    }

    public static NewTryResult<NEWTYPE> NewTry(A value)
    {
        try { return NewTryResult<NEWTYPE>.Succ(New(value)); }
        catch (Exception ex) { return NewTryResult<NEWTYPE>.Fail(ex); }
    }

    public bool Equals(NEWTYPE? other) =>
        other is not null && default(ORD).Equals(Value, other.Value);

    public override bool Equals(object? obj) =>
        obj is NEWTYPE other && Equals(other);

    public override int GetHashCode() =>
        default(ORD).GetHashCode(Value);

    public int CompareTo(NEWTYPE? other) =>
        other is null ? 1 : default(ORD).Compare(Value, other.Value);

    public override string ToString() =>
        Value?.ToString() ?? "";

    public static bool operator ==(NewType<NEWTYPE, A, PRED, ORD>? lhs, NewType<NEWTYPE, A, PRED, ORD>? rhs) =>
        lhs is null ? rhs is null : lhs.Equals(rhs as NEWTYPE);

    public static bool operator !=(NewType<NEWTYPE, A, PRED, ORD>? lhs, NewType<NEWTYPE, A, PRED, ORD>? rhs) =>
        !(lhs == rhs);
}

/// <summary>
/// v4 compatibility shim for <c>NewType&lt;NEWTYPE, A&gt;</c> (2-arg convenience version).
/// Uses <c>True&lt;A&gt;</c> (no predicate) and <c>OrdDefault&lt;A&gt;</c>.
/// </summary>
[Obsolete("NewType is a v4 compatibility shim. Migrate to DomainType<SELF, REPR> or record types.")]
public abstract class NewType<NEWTYPE, A> : NewType<NEWTYPE, A, True<A>, OrdDefault<A>>
    where NEWTYPE : NewType<NEWTYPE, A>
{
    protected NewType(A value) : base(value) { }
}

#pragma warning restore CS0618
