using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.ClassInstances.Pred;

/// <summary>
/// v4 compatibility shim. A predicate that always returns true.
/// </summary>
[Obsolete("v4 type class shim. Predicates were removed in v5.")]
public struct True<A> : Pred<A>
{
    bool Pred<A>.True(A value) => true;
}
