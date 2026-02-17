# LanguageExt v5 Migration Analysis

Analysis of breaking changes when upgrading Dbosoft.Functional from LanguageExt v4 (`[4.4.0,4.5.0)`) to v5 (`5.0.0-beta-77`).

> Target framework also changes: `netstandard2.0` -> `net10.0` (v5 requires .NET 10+)

---

## Build Result: 18 errors, 4 warnings

---

## Breaking Changes in LanguageExt v5

### 1. Removed Types

| Removed | Replacement |
|---------|-------------|
| `EitherAsync<L, R>` | `EitherT<L, IO, R>` (monad transformer over IO) |
| `OptionAsync<A>` | `OptionT<IO, A>` |
| `TryAsync<A>` | `TryT<IO, A>` |
| `TryOption<A>` | `OptionT<Try, A>` |
| `Aff<A>`, `Aff<RT, A>` | `Eff<A>` (now handles both sync and async) |
| `NewType<NEWTYPE, A, PRED, ORD>` | C# `record` types (NewType removed entirely) |
| `Pred<A>`, `True<A>` | Removed (predicate system) |

### 2. Removed Namespaces / Type Classes

| Removed | Replacement |
|---------|-------------|
| `LanguageExt.ClassInstances.Pred` | Removed |
| `LanguageExt.TypeClasses` | `LanguageExt.Traits` (static abstract interface members) |
| `struct, Ord<A>` constraint | `Ord<A>` is now a trait interface, not a struct type class |

### 3. Removed Prelude Functions

| Removed | Replacement |
|---------|-------------|
| `Prelude.RightAsync<L, R>(x)` | `EitherT<L, IO, R>.Right(x)` or `Pure(x)` |
| `Prelude.LeftAsync<L, R>(x)` | `EitherT<L, IO, R>.Left(x)` or `Fail(x)` |
| `Prelude.SomeAsync(x)` | `OptionT<IO, A>.Some(x)` |
| `x.ToAsync()` (on Either/Option) | Wrap in transformer: `EitherT.lift(io)` |

### 4. Effect System Changes

- `Aff<A>` merged into `Eff<A>` (which now supports both sync and async)
- `Eff<RT, A>` no longer requires `HasCancel<RT>` trait
- New `IO<A>` monad is the foundation for all side effects
- `liftIO(async () => ...)` and `liftEff(async () => ...)` for async lifting

### 5. Validation Changes

- `Validation` `|` operator semantics changed (v4 `|` behaved like `&`)
- `ValidationT<F, M, A>` monad transformer added

### 6. Either `bi` Functions

- Arguments to `BiMap`, `BiFold`, etc. are **flipped** (Left/Right handler order swapped)

### 7. Semigroup / Monoid

- Now traits that types must implement directly (static abstract `operator+` and `Empty`)
- Cannot make external types into semigroups/monoids anymore

### 8. Transformers Package Removed

- `LanguageExt.Transformers` deleted (500k+ lines of generated `MapT`, `BindT`, etc.)
- Replaced by generic trait-based methods in `LanguageExt.Core`

---

## Impact on Dbosoft.Functional Source Files

### `EitherExtensions.cs` — 6 errors

All methods using `EitherAsync<,>` are broken:

| Method | Issue | Migration Path |
|--------|-------|----------------|
| `ToEitherRight<TIn>(this TIn)` | Uses `Prelude.RightAsync` (removed) | Return `Either<Error, TIn>` synchronously, or use `EitherT` |
| `ToEitherLeft<TIn>(this Error)` | Uses `Prelude.LeftAsync` (removed) | Return `Either<Error, TIn>` synchronously, or use `EitherT` |
| `IfNoneAsync<TIn>(...)` | Uses `.ToAsync()` (removed) | Rewrite using `Either` + `Eff`/`IO` for async |
| `NoneToError<T>(this EitherAsync<Error, Option<T>>, Error)` | `EitherAsync` removed | Rewrite for `EitherT<Error, IO, Option<T>>` or `Eff<Either<Error, T>>` |
| `SomeToError<T>(...)` (both overloads) | `EitherAsync` removed | Same as NoneToError |

### `AffExtensions.cs` — 3 errors

| Method | Issue | Migration Path |
|--------|-------|----------------|
| `ToAff<R>(this EitherAsync<Error, R>)` | Both `Aff` and `EitherAsync` removed | **Delete entirely** — no direct equivalent needed |
| `ToEitherAsync<R>(this ValueTask<Fin<R>>)` | `EitherAsync` removed | Convert to `Either<Error, R>` from `Fin<R>` (sync), or wrap in `IO`/`Eff` |

### `UseExtensions.cs` — 4 errors

| Method | Issue | Migration Path |
|--------|-------|----------------|
| `Use<L,R1,R2>(this EitherAsync<L,R1>, ...)` | `EitherAsync` removed | Rewrite for `EitherT<L, IO, R1>` or drop in favor of `Eff` resource tracking |
| `Use<L,R1,R2>(this Task<Either<L,R1>>, ...)` | Still compiles (uses `Task<Either>`) | **Keep** — may still work |
| `Use<L,R1,R2>(this Either<L,R1>, ...)` | Still compiles | **Keep** |

> Note: v5 `Eff<A>` has built-in resource tracking with automatic cleanup, which may make `Use` extensions redundant.

### `ValidatingNewType.cs` — 5 errors

| Issue | Detail |
|-------|--------|
| `LanguageExt.ClassInstances.Pred` removed | Entire predicate system gone |
| `LanguageExt.TypeClasses` removed | Replaced by `LanguageExt.Traits` |
| `NewType<NEWTYPE, A, True<A>, ORD>` removed | `NewType` with 4 generic args no longer exists |
| `True<A>` removed | Predicate class gone |
| `struct, Ord<A>` constraint invalid | `Ord<A>` is now a trait, not a struct |

**Migration:** `NewType` is replaced by C# `record` types in v5. The entire `ValidatingNewType` class needs a ground-up redesign — likely as an abstract `record` with a static `Validate` method returning `Validation<Error, NEWTYPE>`.

### `ErrorExtensions.cs` — 0 errors

`Error` type changed from struct to abstract record, but the `Print` method's pattern matching on `ManyErrors` / `Exceptional` should still work since those subtypes exist in v5. **Likely still compiles** but needs verification.

---

## Impact on Dbosoft.Functional.Json

### `NewTypeJsonConverter.cs` — Multiple errors expected

The entire converter depends on `NewType<NEWTYPE, A, PRED, ORD>` which no longer exists in v5. Also uses:
- `Pred<A>` (removed)
- `Ord<A>` as struct constraint (changed to trait)
- `NewType<,,,>` reflection checks

**Migration:** Needs complete rewrite. Since v5 replaces `NewType` with plain C# `record` types, the JSON converter concept needs to be reimagined — possibly as a converter for record types that implement a specific interface, or it may become unnecessary if records use standard JSON serialization.

---

## Impact on Tests

### `FluentAssertions.LanguageExt` — Incompatible

Package `0.5.0` targets LanguageExt v4. Will not work with v5 types. Need to either:
- Find/create a v5-compatible assertion library
- Write custom assertion extensions (like the `AwesomeAssertions.LanguageExt` in SAP2Dynamics)

### Test code using `EitherAsync`, `RightAsync`, `LeftAsync`, `Aff`

All test methods using these types/functions need rewriting.

---

## Decision Points

1. **`EitherAsync` replacement strategy**: Use `EitherT<L, IO, R>` everywhere, or switch to `Eff<Either<L, R>>`/`Eff<R>` with error handling?
2. **`ValidatingNewType` redesign**: Keep the concept with records, or drop it in favor of simple record + static validation?
3. **`NewTypeJsonConverter` redesign**: Rewrite for records, or drop if no longer needed?
4. **`UseExtensions` for `EitherAsync`**: Drop the `EitherAsync` overload, or rewrite for `EitherT`? (v5 `Eff` has built-in resource tracking)
5. **`AffExtensions`**: Delete entirely since both `Aff` and `EitherAsync` are gone?
6. **Test assertion library**: Write custom v5 assertions or wait for community package?
