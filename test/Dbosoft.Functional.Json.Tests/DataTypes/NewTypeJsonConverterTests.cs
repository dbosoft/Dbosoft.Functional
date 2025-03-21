using Dbosoft.Functional.Json.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Functional.DataTypes;
using FluentAssertions;
using LanguageExt;
using LanguageExt.ClassInstances;

namespace Dbosoft.Functional.Json.Tests.DataTypes;

public class NewTypeJsonConverterTests
{
    private static JsonSerializerOptions Options => new(JsonSerializerDefaults.Web)
    {
        Converters = { new NewTypeJsonConverter() },
    };

    private static JsonSerializerOptions OptionsWithIndent => new(JsonSerializerDefaults.Web)
    {
        Converters = { new NewTypeJsonConverter() },
        WriteIndented = true,
    };

    [Fact]
    public void Deserialize_StringNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": "property-value",
                     "list": [
                       "list-item-1",
                       "list-item-2"
                     ],
                     "map": {
                       "key-1": "map-value-1",
                       "key-2": "map-value-2"
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<KeyTestEntity<TestType<string>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be("property-value");
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be("list-item-1"),
            item => item.Value.Should().Be("list-item-2"));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey(TestType<string>.New("key-1"))
            .WhoseValue.Value.Should().Be("map-value-1");
        result.Map.Should().ContainKey(TestType<string>.New("key-2"))
            .WhoseValue.Value.Should().Be("map-value-2");
    }

    [Fact]
    public void Deserialize_ValidatingNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": "property-value",
                     "list": [
                       "list-item-1",
                       "list-item-2"
                     ],
                     "map": {
                       "key-1": "map-value-1",
                       "key-2": "map-value-2"
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<KeyTestEntity<ValidatingTestType>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be("property-value");
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be("list-item-1"),
            item => item.Value.Should().Be("list-item-2"));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey(ValidatingTestType.New("key-1"))
            .WhoseValue.Value.Should().Be("map-value-1");
        result.Map.Should().ContainKey(ValidatingTestType.New("key-2"))
            .WhoseValue.Value.Should().Be("map-value-2");
    }

    [Fact]
    public void Deserialize_IntNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 42,
                     "list": [
                       43,
                       -44
                     ],
                     "map": {
                       "key-1": 45,
                       "key-2": -46
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<int>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(42);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(43),
            item => item.Value.Should().Be(-44));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(45);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(-46);
    }

    [Fact]
    public void Serialize_StringNewType_ReturnsJson()
    {
        var entity = new KeyTestEntity<TestType<string>>
        {
            Property = TestType<string>.New("property-value"),
            List =
            [
                TestType<string>.New("list-item-1"),
                TestType<string>.New("list-item-2")
            ],
            Map = new Dictionary<TestType<string>, TestType<string>>
            {
                [TestType<string>.New("key-1")] = TestType<string>.New("map-value-1"),
                [TestType<string>.New("key-2")] = TestType<string>.New("map-value-2")
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);
        
        result.Should().Be("""
                          {
                            "property": "property-value",
                            "list": [
                              "list-item-1",
                              "list-item-2"
                            ],
                            "map": {
                              "key-1": "map-value-1",
                              "key-2": "map-value-2"
                            }
                          }
                          """);
    }

    [Fact]
    public void Serialize_ValidatingNewType_ReturnsJson()
    {
        var entity = new KeyTestEntity<ValidatingTestType>
        {
            Property = ValidatingTestType.New("property-value"),
            List =
            [
                ValidatingTestType.New("list-item-1"),
                ValidatingTestType.New("list-item-2")
            ],
            Map = new Dictionary<ValidatingTestType, ValidatingTestType>
            {
                [ValidatingTestType.New("key-1")] = ValidatingTestType.New("map-value-1"),
                [ValidatingTestType.New("key-2")] = ValidatingTestType.New("map-value-2")
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": "property-value",
                             "list": [
                               "list-item-1",
                               "list-item-2"
                             ],
                             "map": {
                               "key-1": "map-value-1",
                               "key-2": "map-value-2"
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_IntNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<int>>
        {
            Property = TestType<int>.New(42),
            List =
            [
                TestType<int>.New(43),
                TestType<int>.New(-44)
            ],
            Map = new Dictionary<string, TestType<int>>
            {
                ["key-1"] = TestType<int>.New(45),
                ["key-2"] = TestType<int>.New(-46)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 42,
                             "list": [
                               43,
                               -44
                             ],
                             "map": {
                               "key-1": 45,
                               "key-2": -46
                             }
                           }
                           """);
    }

    public class TestType<TValue>(TValue value) : NewType<TestType<TValue>, TValue>(value)
    {
    }

    public class ValidatingTestType(string value)
        : ValidatingNewType<ValidatingTestType, string, OrdStringOrdinalIgnoreCase>(value)
    {
    }

    public class KeyTestEntity<TValue>
    {
        public required TValue Property { get; set; }
        public required IReadOnlyList<TValue> List { get; set; }
        public required IReadOnlyDictionary<TValue, TValue> Map { get; init; }
    }

    public class TestEntity<TValue>
    {
        public required TValue Property { get; set; }

        public required IReadOnlyList<TValue> List { get; set; }

        public required IReadOnlyDictionary<string, TValue> Map { get; init; }
    }
}

