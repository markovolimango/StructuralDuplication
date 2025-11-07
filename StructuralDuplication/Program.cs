using Microsoft.CodeAnalysis.CSharp;

namespace StructuralDuplication;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.Write("Enter file path: ");
        var path = Console.ReadLine();
        if (!File.Exists(path))
        {
            Console.WriteLine("File not found.");
            return;
        }

        var text = File.ReadAllText(path);

        try
        {
            var tree = CSharpSyntaxTree.ParseText(text);

            var rewriter = new SingleParamMethodRewriter();
            var newTree = rewriter.Visit(tree.GetRoot());

            File.WriteAllText(path, newTree.ToFullString());
        }
        catch
        {
            Console.WriteLine("Couldn't parse file.");
        }
    }
}