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
    public void Deserialize_BoolNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": true,
                     "list": [
                       true,
                       false
                     ],
                     "map": {
                       "key-1": true,
                       "key-2": false
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<bool>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(true);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(true),
            item => item.Value.Should().Be(false));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(true);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(false);
    }

    [Fact]
    public void Deserialize_ShortNewType_ReturnsData()
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
        var result = JsonSerializer.Deserialize<TestEntity<TestType<short>>>(json, Options);
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
    public void Deserialize_LongNewType_ReturnsData()
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
        var result = JsonSerializer.Deserialize<TestEntity<TestType<long>>>(json, Options);
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
    public void Deserialize_ByteShortNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 42,
                     "list": [
                       43,
                       44
                     ],
                     "map": {
                       "key-1": 45,
                       "key-2": 46
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<byte>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(42);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(43),
            item => item.Value.Should().Be(44));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(45);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(46);
    }

    [Fact]
    public void Deserialize_UnsignedShortNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 42,
                     "list": [
                       43,
                       44
                     ],
                     "map": {
                       "key-1": 45,
                       "key-2": 46
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<ushort>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(42);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(43),
            item => item.Value.Should().Be(44));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(45);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(46);
    }

    [Fact]
    public void Deserialize_UnsignedIntNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 42,
                     "list": [
                       43,
                       44
                     ],
                     "map": {
                       "key-1": 45,
                       "key-2": 46
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<uint>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(42);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(43),
            item => item.Value.Should().Be(44));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(45);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(46);
    }

    [Fact]
    public void Deserialize_UnsignedLongNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 42,
                     "list": [
                       43,
                       44
                     ],
                     "map": {
                       "key-1": 45,
                       "key-2": 46
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<ulong>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(42);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(43),
            item => item.Value.Should().Be(44));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(45);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(46);
    }

    [Fact]
    public void Deserialize_FloatNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 4.2,
                     "list": [
                       4.3,
                       -4.4
                     ],
                     "map": {
                       "key-1": 4.5,
                       "key-2": -4.6
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<float>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(4.2f);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(4.3f),
            item => item.Value.Should().Be(-4.4f));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(4.5f);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(-4.6f);
    }

    [Fact]
    public void Deserialize_DoubleNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 4.2,
                     "list": [
                       4.3,
                       -4.4
                     ],
                     "map": {
                       "key-1": 4.5,
                       "key-2": -4.6
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<double>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(4.2);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(4.3),
            item => item.Value.Should().Be(-4.4));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(4.5);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(-4.6);
    }

    [Fact]
    public void Deserialize_DecimalNewType_ReturnsData()
    {
        var json = """
                   {
                     "property": 4.2,
                     "list": [
                       4.3,
                       -4.4
                     ],
                     "map": {
                       "key-1": 4.5,
                       "key-2": -4.6
                     }
                   }
                   """;
        var result = JsonSerializer.Deserialize<TestEntity<TestType<decimal>>>(json, Options);
        result.Should().NotBeNull();
        result!.Property.Value.Should().Be(4.2m);
        result.List.Should().SatisfyRespectively(
            item => item.Value.Should().Be(4.3m),
            item => item.Value.Should().Be(-4.4m));
        result.Map.Should().HaveCount(2);
        result.Map.Should().ContainKey("key-1")
            .WhoseValue.Value.Should().Be(4.5m);
        result.Map.Should().ContainKey("key-2")
            .WhoseValue.Value.Should().Be(-4.6m);
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
    public void Serialize_BoolNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<bool>>
        {
            Property = TestType<bool>.New(true),
            List =
            [
                TestType<bool>.New(true),
                TestType<bool>.New(false)
            ],
            Map = new Dictionary<string, TestType<bool>>
            {
                ["key-1"] = TestType<bool>.New(true),
                ["key-2"] = TestType<bool>.New(false)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": true,
                             "list": [
                               true,
                               false
                             ],
                             "map": {
                               "key-1": true,
                               "key-2": false
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_ShortNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<short>>
        {
            Property = TestType<short>.New(42),
            List =
            [
                TestType<short>.New(43),
                TestType<short>.New(-44)
            ],
            Map = new Dictionary<string, TestType<short>>
            {
                ["key-1"] = TestType<short>.New(45),
                ["key-2"] = TestType<short>.New(-46)
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

    [Fact]
    public void Serialize_LongNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<long>>
        {
            Property = TestType<long>.New(42),
            List =
            [
                TestType<long>.New(43),
                TestType<long>.New(-44)
            ],
            Map = new Dictionary<string, TestType<long>>
            {
                ["key-1"] = TestType<long>.New(45),
                ["key-2"] = TestType<long>.New(-46)
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

    [Fact]
    public void Serialize_ByteNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<byte>>
        {
            Property = TestType<byte>.New(42),
            List =
            [
                TestType<byte>.New(43),
                TestType<byte>.New(44)
            ],
            Map = new Dictionary<string, TestType<byte>>
            {
                ["key-1"] = TestType<byte>.New(45),
                ["key-2"] = TestType<byte>.New(46)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 42,
                             "list": [
                               43,
                               44
                             ],
                             "map": {
                               "key-1": 45,
                               "key-2": 46
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_UnsignedShortNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<ushort>>
        {
            Property = TestType<ushort>.New(42),
            List =
            [
                TestType<ushort>.New(43),
                TestType<ushort>.New(44)
            ],
            Map = new Dictionary<string, TestType<ushort>>
            {
                ["key-1"] = TestType<ushort>.New(45),
                ["key-2"] = TestType<ushort>.New(46)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 42,
                             "list": [
                               43,
                               44
                             ],
                             "map": {
                               "key-1": 45,
                               "key-2": 46
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_UnsignedIntNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<uint>>
        {
            Property = TestType<uint>.New(42),
            List =
            [
                TestType<uint>.New(43),
                TestType<uint>.New(44)
            ],
            Map = new Dictionary<string, TestType<uint>>
            {
                ["key-1"] = TestType<uint>.New(45),
                ["key-2"] = TestType<uint>.New(46)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 42,
                             "list": [
                               43,
                               44
                             ],
                             "map": {
                               "key-1": 45,
                               "key-2": 46
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_UnsignedLongNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<ulong>>
        {
            Property = TestType<ulong>.New(42),
            List =
            [
                TestType<ulong>.New(43),
                TestType<ulong>.New(44)
            ],
            Map = new Dictionary<string, TestType<ulong>>
            {
                ["key-1"] = TestType<ulong>.New(45),
                ["key-2"] = TestType<ulong>.New(46)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 42,
                             "list": [
                               43,
                               44
                             ],
                             "map": {
                               "key-1": 45,
                               "key-2": 46
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_FloatNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<float>>
        {
            Property = TestType<float>.New(4.2f),
            List =
            [
                TestType<float>.New(4.3f),
                TestType<float>.New(-4.4f)
            ],
            Map = new Dictionary<string, TestType<float>>
            {
                ["key-1"] = TestType<float>.New(4.5f),
                ["key-2"] = TestType<float>.New(-4.6f)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 4.2,
                             "list": [
                               4.3,
                               -4.4
                             ],
                             "map": {
                               "key-1": 4.5,
                               "key-2": -4.6
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_DoubleNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<double>>
        {
            Property = TestType<double>.New(4.2),
            List =
            [
                TestType<double>.New(4.3),
                TestType<double>.New(-4.4)
            ],
            Map = new Dictionary<string, TestType<double>>
            {
                ["key-1"] = TestType<double>.New(4.5),
                ["key-2"] = TestType<double>.New(-4.6)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 4.2,
                             "list": [
                               4.3,
                               -4.4
                             ],
                             "map": {
                               "key-1": 4.5,
                               "key-2": -4.6
                             }
                           }
                           """);
    }

    [Fact]
    public void Serialize_DecimalNewType_ReturnsJson()
    {
        var entity = new TestEntity<TestType<decimal>>
        {
            Property = TestType<decimal>.New(4.2m),
            List =
            [
                TestType<decimal>.New(4.3m),
                TestType<decimal>.New(-4.4m)
            ],
            Map = new Dictionary<string, TestType<decimal>>
            {
                ["key-1"] = TestType<decimal>.New(4.5m),
                ["key-2"] = TestType<decimal>.New(-4.6m)
            }
        };

        var result = JsonSerializer.Serialize(entity, OptionsWithIndent);

        result.Should().Be("""
                           {
                             "property": 4.2,
                             "list": [
                               4.3,
                               -4.4
                             ],
                             "map": {
                               "key-1": 4.5,
                               "key-2": -4.6
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

