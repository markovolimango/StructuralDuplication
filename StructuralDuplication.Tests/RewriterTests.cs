using Microsoft.CodeAnalysis.CSharp;

namespace StructuralDuplication.Tests;

public class SingleParamMethodRewriterTests
{
    private static string Rewrite(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        var rewriter = new SingleParamMethodRewriter(false);
        var newRoot = rewriter.Visit(root)!;

        return newRoot.ToFullString();
    }

    [Test]
    public void DoesNotModify_MultipleParameters()
    {
        var code = """
                   class Test {
                       void Foo(int x, int y) { }
                   }
                   """;

        var newCode = Rewrite(code);

        Assert.That(newCode, Is.EqualTo(code));
    }

    [Test]
    public void AddsNewParameter_WhenSingleParameter()
    {
        var code = """
                   class Test {
                       void Foo(int param) { }
                   }
                   """;

        var expected = """
                       class Test {
                           void Foo(int param, int param1) { }
                       }
                       """;

        var newCode = Rewrite(code);
        Assert.That(newCode, Is.EqualTo(expected));
    }

    [Test]
    public void AddsNewParameter_WhenMultipleMethods()
    {
        var code = """
                   class Test {
                       void Foo(int param) { }
                       void Bar(int param) { }
                   }
                   """;

        var expected = """
                       class Test {
                           void Foo(int param, int param1) { }
                           void Bar(int param, int param1) { }
                       }
                       """;

        var newCode = Rewrite(code);
        Assert.That(newCode, Is.EqualTo(expected));
    }

    [Test]
    public void SuggestsIncrementedNumericSuffix()
    {
        var code = """
                   class Test {
                       void Foo(int x2) { }
                   }
                   """;

        var expected = """
                       class Test {
                           void Foo(int x2, int x3) { }
                       }
                       """;

        var newCode = Rewrite(code);
        Assert.That(newCode, Is.EqualTo(expected));
    }

    [Test]
    public void SuggestsNextLetter_ForSingleLetterParameter()
    {
        var code = """
                   class Test {
                       void Foo(int a) { }
                   }
                   """;

        var expected = """
                       class Test {
                           void Foo(int a, int b) { }
                       }
                       """;

        var newCode = Rewrite(code);
        Assert.That(newCode, Is.EqualTo(expected));
    }

    [Test]
    public void SuggestsAppended1_ForOtherNames()
    {
        var code = """
                   class Test {
                       void Foo(int value) { }
                   }
                   """;

        var expected = """
                       class Test {
                           void Foo(int value, int value1) { }
                       }
                       """;

        var newCode = Rewrite(code);
        Assert.That(newCode, Is.EqualTo(expected));
    }

    [Test]
    public void DoesNotModify_MethodWithNoParameters()
    {
        var code = """
                   class Test {
                       void Foo() { }
                   }
                   """;

        var newCode = Rewrite(code);

        Assert.That(newCode, Is.EqualTo(code));
    }
}