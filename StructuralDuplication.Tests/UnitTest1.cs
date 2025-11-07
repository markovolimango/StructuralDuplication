using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StructuralDuplication.Tests;

[TestFixture]
public class SingleParamMethodRewriterTests
{
    #region Setup/Teardown

    [SetUp]
    public void Setup()
    {
        _rewriter = new SingleParamMethodRewriter();
    }

    #endregion

    private SingleParamMethodRewriter _rewriter;

    [Test]
    public void VisitMethodDeclaration_WithAttribute_PreservesAttribute()
    {
        // Arrange
        var source = "public void TestMethod([NotNull] string value) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = (MethodDeclarationSyntax)_rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        var firstParam = result.ParameterList.Parameters.First();
        Assert.That(firstParam.AttributeLists.Count, Is.GreaterThan(0));
    }

    [Test]
    public void VisitMethodDeclaration_WithComplexType_PreservesType()
    {
        // Arrange
        var source = "public void TestMethod(Dictionary<string, int> data) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = (MethodDeclarationSyntax)_rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        var firstParam = result.ParameterList.Parameters.First();
        Assert.That(firstParam.Type.ToString(), Is.EqualTo("Dictionary<string, int>"));
    }

    [Test]
    public void VisitMethodDeclaration_WithNoParameters_ReturnsUnchangedNode()
    {
        // Arrange
        var source = "public void TestMethod() { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = _rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        Assert.That(result, Is.EqualTo(methodNode));
        Assert.That(((MethodDeclarationSyntax)result).ParameterList.Parameters.Count, Is.EqualTo(0));
    }

    [Test]
    public void VisitMethodDeclaration_WithSingleParameter_DoesNotModifyOriginalNode()
    {
        // Arrange
        var source = "public void TestMethod(int value) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = _rewriter.Visit(methodNode);

        // Assert
        Assert.That(result, Is.Not.SameAs(methodNode), "Should return a new node instance");
    }

    [Test]
    public void VisitMethodDeclaration_WithSingleParameter_PreservesOriginalParameter()
    {
        // Arrange
        var source = "public void TestMethod(int value) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var originalParam = methodNode.ParameterList.Parameters.First();

        // Act - Note: This test would require mocking console input
        // For now, we'll just verify the structure
        var result = (MethodDeclarationSyntax)_rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        Assert.That(result.ParameterList.Parameters.Count, Is.GreaterThan(1));
        var firstParam = result.ParameterList.Parameters.First();
        Assert.That(firstParam.Identifier.Text, Is.EqualTo(originalParam.Identifier.Text));
        Assert.That(firstParam.Type.ToString(), Is.EqualTo(originalParam.Type.ToString()));
    }

    [Test]
    public void VisitMethodDeclaration_WithSingleParameterWithDefault_PreservesDefault()
    {
        // Arrange
        var source = "public void TestMethod(int value = 42) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = (MethodDeclarationSyntax)_rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        var firstParam = result.ParameterList.Parameters.First();
        Assert.That(firstParam.Default, Is.Not.Null);
        Assert.That(firstParam.Default.Value.ToString(), Is.EqualTo("42"));
    }

    [Test]
    public void VisitMethodDeclaration_WithSingleParameterWithModifiers_PreservesModifiers()
    {
        // Arrange
        var source = "public void TestMethod(ref int value) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = (MethodDeclarationSyntax)_rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        var firstParam = result.ParameterList.Parameters.First();
        Assert.That(firstParam.Modifiers.Count, Is.EqualTo(1));
        Assert.That(firstParam.Modifiers.First().Text, Is.EqualTo("ref"));
    }

    [Test]
    public void VisitMethodDeclaration_WithTwoParameters_ReturnsUnchangedNode()
    {
        // Arrange
        var source = "public void TestMethod(int value1, string value2) { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = _rewriter.VisitMethodDeclaration(methodNode);

        // Assert
        Assert.That(result, Is.EqualTo(methodNode));
        Assert.That(((MethodDeclarationSyntax)result).ParameterList.Parameters.Count, Is.EqualTo(2));
    }
}