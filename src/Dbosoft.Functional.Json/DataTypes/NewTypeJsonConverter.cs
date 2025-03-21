using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.TypeClasses;

namespace Dbosoft.Functional.Json.DataTypes;

/// <summary>
/// <para>
/// A JSON converter which supports <see cref="NewType{NEWTYPE,A, PRED, ORD}"/>.
/// </para>
/// <para>
/// The <see cref="NewType{NEWTYPE,A, PRED, ORD}"/> must be based on one of the
/// following types: <type><see cref="string"/></type>,
/// <type><see cref="bool"/></type>, <type><see cref="short"/></type>
/// <type><see cref="int"/></type>, <type><see cref="long"/></type>
/// <type><see cref="byte"/></type>, <type><see cref="ushort"/></type>
/// <type><see cref="uint"/></type>, <type><see cref="ulong"/></type>,
/// <type><see cref="float"/></type>, <type><see cref="double"/></type>,
/// <type><see cref="decimal"/></type>.
/// </para>
/// <para>
/// <see cref="NewType{NEWTYPE,A, PRED, ORD}"/>s with <see cref="String"/> as
/// the base value can be used as keys in dictionaries, objects, etc.
/// </para>
/// </summary>
public class NewTypeJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        var newType = FindNewType(typeToConvert);
        if (newType is null || !newType.IsGenericType || newType.GetGenericTypeDefinition() != typeof(NewType<,,,>))
            return false;

        var arguments = newType.GetGenericArguments();
        if (arguments.Length != 4)
            return false;

        var a = arguments[1];
        return a == typeof(string) || a == typeof(bool)
               || a == typeof(short) || a == typeof(int) || a == typeof(long)
               || a == typeof(byte) || a == typeof(ushort) || a == typeof(uint) || a == typeof(ulong)
               || a == typeof(float) || a == typeof(double) || a == typeof(decimal);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var newType = FindNewType(typeToConvert);
        if (newType is null || !newType.IsGenericType || newType.GetGenericTypeDefinition() != typeof(NewType<,,,>))
            throw new ArgumentException("The type is not a LanguageExt NewType.", nameof(typeToConvert));

        var arguments = newType.GetGenericArguments();
        var a = arguments[1];
        var pred = arguments[2];
        var ord = arguments[3];

        if (a == typeof(string))
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(NewTypeJsonConverter<,,,>)
                    .MakeGenericType(typeToConvert, typeToConvert, pred, ord));
        }

        if (a == typeof(bool) || a == typeof(short) || a == typeof(int) || a == typeof(long)
            || a == typeof(byte) || a == typeof(ushort) || a == typeof(uint) || a == typeof(ulong)
            || a == typeof(float) || a == typeof(double) || a == typeof(decimal))
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(NewTypeJsonConverter<,,,,>)
                    .MakeGenericType(typeToConvert, typeToConvert, a, pred, ord));
        }

        throw new ArgumentException($"The value type {a.Name} is not supported.", nameof(typeToConvert));
    }

    private static Type? FindNewType(Type type)
    {
        var candidate = type;
        while (candidate is not null && candidate != typeof(object))
        {
            if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(NewType<,,,>))
                return candidate;
            candidate = candidate.BaseType;
        }

        return null;
    }
}

internal class NewTypeJsonConverter<T, NEWTYPE, PRED, ORD>
    : JsonConverter<T>
    where T : NewType<NEWTYPE, string, PRED, ORD>
    where PRED : struct, Pred<string>
    where NEWTYPE : NewType<NEWTYPE, string, PRED, ORD>
    where ORD : struct, Ord<string>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (T)Activator.CreateInstance(typeToConvert, reader.GetString());
    }

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (T)Activator.CreateInstance(typeToConvert, reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value);
    }
}

internal class NewTypeJsonConverter<T, NEWTYPE, A, PRED, ORD>
    : JsonConverter<T>
    where T : NewType<NEWTYPE, A, PRED, ORD>
    where PRED : struct, Pred<A>
    where NEWTYPE : NewType<NEWTYPE, A, PRED, ORD>
    where ORD : struct, Ord<A>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object? value = typeof(A) switch
        {
            { } t when t == typeof(bool) => reader.GetBoolean(),
            { } t when t == typeof(byte) => reader.GetByte(),
            { } t when t == typeof(short) => reader.GetInt16(),
            { } t when t == typeof(int) => reader.GetInt32(),
            { } t when t == typeof(long) => reader.GetInt64(),
            { } t when t == typeof(ushort) => reader.GetUInt16(),
            { } t when t == typeof(uint) => reader.GetUInt32(),
            { } t when t == typeof(ulong) => reader.GetUInt64(),
            { } t when t == typeof(float) => reader.GetSingle(),
            { } t when t == typeof(double) => reader.GetDouble(),
            { } t when t == typeof(decimal) => reader.GetDecimal(),
            _ => throw new InvalidOperationException($"Values of type {typeof(A).Name} are not supported.")
        };
        return (T)Activator.CreateInstance(typeToConvert, value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        switch (value.Value)
        {
            case bool v:
                writer.WriteBooleanValue(v);
                return;
            case byte v:
                writer.WriteNumberValue(v);
                return;
            case short v:
                writer.WriteNumberValue(v);
                return;
            case int v:
                writer.WriteNumberValue(v);
                return;
            case long v:
                writer.WriteNumberValue(v);
                return;
            case ushort v:
                writer.WriteNumberValue(v);
                return;
            case uint v:
                writer.WriteNumberValue(v);
                return;
            case ulong v:
                writer.WriteNumberValue(v);
                break;
            case float v:
                writer.WriteNumberValue(v);
                return;
            case double v:
                writer.WriteNumberValue(v);
                return;
            case decimal v:
                writer.WriteNumberValue(v);
                return;
            default:
                throw new ArgumentException("Values this type are not supported.", nameof(value));
        }
    }
}
