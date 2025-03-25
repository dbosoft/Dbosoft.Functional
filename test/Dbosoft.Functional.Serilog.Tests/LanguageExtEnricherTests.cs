using Serilog;
using FluentAssertions;
using LanguageExt.Common;
using Serilog.Core;

namespace Dbosoft.Functional.Serilog.Tests;

public class LanguageExtEnricherTests
{
    [Fact]
    public void Enriches_log_with_complex_error_hierarchy()
    {
        using var writer = new StringWriter();
        using (var log = CreateLogger(writer))
        {
            try
            {
                throw new Exception("test exception");
            }
            catch (Exception ex)
            {
                var error = Error.New("root error",
                    Error.Many(
                        Error.New("outer error",
                            Error.New("inner error")),
                        Error.New(ex)));
                log.Error(error, "log message");
            }
        }

        var result = writer.ToString();
        result.Should().StartWith(
            """
            [ERR] log message
            root error
            outer error
            inner error
            System.Exception: test exception
            """);
    }

    [Fact]
    public void Enriches_log_with_nested_exception()
    {
        using var writer = new StringWriter();
        using (var log = CreateLogger(writer))
        {
            try
            {
                try
                {
                    throw new Exception("inner exception");
                }
                catch (Exception ex)
                {
                    throw new Exception("outer exception", ex);
                }
            }
            catch (Exception ex)
            {
                log.Error(Error.New("root error", Error.New(ex)), "log message");
            }
        }

        var result = writer.ToString();
        result.Should().StartWith(
            """
            [ERR] log message
            root error
            System.Exception: outer exception
             ---> System.Exception: inner exception
            """);
    }

    private static Logger CreateLogger(TextWriter writer)
    {
        return new LoggerConfiguration()
            .Enrich.WithLanguageExt()
            .WriteTo.TextWriter(
                writer,
                outputTemplate: "[{Level:u3}] {Message}{NewLine}{Exception}{InnerError}")
            .CreateLogger();
    }
}
