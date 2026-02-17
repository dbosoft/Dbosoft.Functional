using System;
using System.Collections.Generic;
using LanguageExt.TypeClasses;

namespace LanguageExt.ClassInstances;

/// <summary>
/// v4 compatibility shim. Default ordering using <see cref="Comparer{T}.Default"/>.
/// </summary>
[Obsolete("v4 type class shim. Use v5 traits instead.")]
public struct OrdDefault<A> : Ord<A>
{
    public int Compare(A x, A y) => Comparer<A>.Default.Compare(x, y);
    public bool Equals(A x, A y) => EqualityComparer<A>.Default.Equals(x, y);
    public int GetHashCode(A x) => x is null ? 0 : EqualityComparer<A>.Default.GetHashCode(x);
}

/// <summary>
/// v4 compatibility shim. Case-insensitive ordinal string ordering.
/// </summary>
[Obsolete("v4 type class shim. Use v5 traits instead.")]
public struct OrdStringOrdinalIgnoreCase : Ord<string>
{
    public int Compare(string x, string y) => StringComparer.OrdinalIgnoreCase.Compare(x, y);
    public bool Equals(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
    public int GetHashCode(string x) => StringComparer.OrdinalIgnoreCase.GetHashCode(x);
}
