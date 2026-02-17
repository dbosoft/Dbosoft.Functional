using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LanguageExt.Common;
using LanguageExt.Traits;

namespace Dbosoft.Functional.Validations;

/// <summary>
/// Represents an issue which was detected during a validation.
/// When multiple issues are combined via the Monoid operator,
/// the individual issues can be iterated.
/// </summary>
public readonly struct ValidationIssue : Monoid<ValidationIssue>, IEnumerable<ValidationIssue>
{
    private readonly string _member;
    private readonly string _message;
    private readonly ValidationIssue[] _issues;

    /// <summary>
    /// Creates a new validation issue.
    /// </summary>
    public ValidationIssue(string member, string message)
    {
        _member = member;
        _message = message;
        _issues = null;
    }

    private ValidationIssue(ValidationIssue[] issues)
    {
        _member = null;
        _message = null;
        _issues = issues;
    }

    /// <summary>
    /// The path to the member in the object tree which caused the issue,
    /// e.g. <c>Participants[2].Name</c>.
    /// </summary>
    public string Member => _member ?? "";

    /// <summary>
    /// The description of the issue.
    /// </summary>
    public string Message => _message ?? "";

    /// <summary>
    /// Converts the issue to an <see cref="Error"/>.
    /// </summary>
    public Error ToError() => Error.New(ToString());

    /// <summary>
    /// Returns a string representation of the issue.
    /// </summary>
    public override string ToString() =>
        _issues is not null
            ? string.Join("; ", _issues.Select(i => i.ToString()))
            : string.IsNullOrWhiteSpace(_member) ? _message ?? "" : $"{_member}: {_message}";

    /// <summary>
    /// The empty/identity element for the Monoid.
    /// </summary>
    public static ValidationIssue Empty => default;

    /// <summary>
    /// Combines two validation issues, preserving individual issues for iteration.
    /// </summary>
    public static ValidationIssue operator +(ValidationIssue a, ValidationIssue b)
    {
        var aEmpty = a._message is null && a._issues is null;
        var bEmpty = b._message is null && b._issues is null;
        if (aEmpty) return b;
        if (bEmpty) return a;

        var left = a._issues ?? new[] { a };
        var right = b._issues ?? new[] { b };
        var combined = new ValidationIssue[left.Length + right.Length];
        left.CopyTo(combined, 0);
        right.CopyTo(combined, left.Length);
        return new ValidationIssue(combined);
    }

    /// <summary>
    /// Combines this validation issue with another (Semigroup implementation).
    /// </summary>
    public ValidationIssue Combine(ValidationIssue other) => this + other;

    /// <inheritdoc />
    public IEnumerator<ValidationIssue> GetEnumerator()
    {
        if (_message is null && _issues is null)
            yield break;

        if (_issues is not null)
        {
            foreach (var issue in _issues)
                yield return issue;
        }
        else
        {
            yield return this;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
