using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StructuralDuplication;

public class SingleParamMethodRewriter : CSharpSyntaxRewriter
{
    private readonly bool _allowUserInput;

    public SingleParamMethodRewriter(bool allowUserInput = true)
    {
        _allowUserInput = allowUserInput;
    }

    /// <summary>
    ///     Adds a new parameter to all single parameter method declarations.
    /// </summary>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.ParameterList.Parameters.Count != 1)
            return node;

        var parameter = node.ParameterList.Parameters.First();
        var name = parameter.Identifier.Text;

        var suggestedName = SuggestNewName(name);
        var newName = suggestedName;
        if (_allowUserInput)
            newName = GetUserInput(name, node.Identifier.Text, suggestedName);

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

    private static string SuggestNewName(string oldName)
    {
        if (char.IsDigit(oldName[^1]))
        {
            var i = oldName.Length - 1;
            while (i >= 0 && char.IsDigit(oldName[i]))
                i--;
            var newNum = int.Parse(oldName.Substring(i + 1)) + 1;
            return oldName[..(i + 1)] + newNum;
        }

        if (oldName.Length == 1 && char.IsLetter(oldName[0]) && oldName[0] != 'z')
        {
            var newChar = (char)(oldName[0] + 1);
            return newChar.ToString();
        }

        return oldName + "1";
    }

    private static string GetUserInput(string oldName, string methodName, string suggestedName)
    {
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