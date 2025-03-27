using LanguageExt.Common;

namespace Dbosoft.Functional.Tests;

public class ErrorExtensionsTests
{
    [Fact]
    public void Print_ComplexHierarchy_ReturnsString()
    {
        Error error;
        try
        {
            throw new Exception("test exception");
        }
        catch (Exception ex)
        {
            error = Error.New("root error",
                Error.Many(
                    Error.New("outer error",
                        Error.New("inner error")),
                    Error.New(ex)));
        }

        var result = error.Print();
        result.Should().StartWith(
            """
            root error
            outer error
            inner error
            System.Exception: test exception
            """);
    }

    [Fact]
    public void Print_NestedException_ReturnsString()
    {
        Error error;
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
            error = Error.New("root error", Error.New(ex));
        }

        var result = error.Print(); 
        result.Should().StartWith(
            """
            root error
            System.Exception: outer exception
             ---> System.Exception: inner exception
            """);
    }
}
