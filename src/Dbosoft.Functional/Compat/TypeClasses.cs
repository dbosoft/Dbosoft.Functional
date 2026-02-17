using System;

namespace LanguageExt.TypeClasses;

/// <summary>
/// v4 compatibility shim for the Eq type class.
/// </summary>
[Obsolete("v4 type class shim. Use IEquatable or v5 traits instead.")]
public interface Eq<A>
{
    bool Equals(A x, A y);
    int GetHashCode(A x);
}

/// <summary>
/// v4 compatibility shim for the Ord type class.
/// </summary>
[Obsolete("v4 type class shim. Use IComparable or v5 traits instead.")]
public interface Ord<A> : Eq<A>
{
    int Compare(A x, A y);
}

/// <summary>
/// v4 compatibility shim for the Pred type class.
/// </summary>
[Obsolete("v4 type class shim. Predicates were removed in v5.")]
public interface Pred<A>
{
    bool True(A value);
}
