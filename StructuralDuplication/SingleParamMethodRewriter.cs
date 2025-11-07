using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StructuralDuplication;

public class SingleParamMethodRewriter : CSharpSyntaxRewriter
{
    /// <summary>
    ///     Adds a new parameter to all single parameter method declarations.
    /// </summary>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.ParameterList.Parameters.Count != 1)
            return node;

        var parameter = node.ParameterList.Parameters.First();
        var name = parameter.Identifier.Text;

        var newName = InputNewName(name, node.Identifier.Text);
        var newParameter = SyntaxFactory.Parameter(
                parameter.AttributeLists,
                parameter.Modifiers,
                parameter.Type,
                SyntaxFactory.Identifier(newName),
                parameter.Default
            )
            .WithLeadingTrivia(SyntaxFactory.Space)
            .WithTrailingTrivia(parameter.GetTrailingTrivia());

        return node.WithParameterList(node.ParameterList.AddParameters(newParameter));
    }

    /// <summary>
    ///     Gets user input for new parameter name.
    /// </summary>
    private static string InputNewName(string oldName, string methodName)
    {
        var suggestedName = oldName + "1";
        Console.Write(
            $"\nDuplicating parameter {oldName} in method {methodName}. Suggested name: {suggestedName}.\n" +
            "Enter to accept, or type new name: ");
        var newName = Console.ReadLine();
        while (true)
        {
            if (newName is null || newName.Length == 0)
                return suggestedName;
            if (SyntaxFacts.IsValidIdentifier(newName))
                return newName;
            Console.Write("Invalid name. Try again: ");
            newName = Console.ReadLine();
        }
    }
}