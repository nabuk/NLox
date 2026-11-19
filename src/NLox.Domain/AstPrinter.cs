using System.Text;

namespace NLox.Domain;

public class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }
    
    string Expr.IVisitor<string>.VisitBinaryExpr(Expr.Binary expr) =>
        Parenthesize(expr.Operator.Lexeme,  expr.Left, expr.Right);

    string Expr.IVisitor<string>.VisitGroupingExpr(Expr.Grouping expr) =>
        Parenthesize("group", expr.Expression);

    string Expr.IVisitor<string>.VisitLiteralExpr(Expr.Literal expr) =>
        expr.Value?.ToString() ?? "nil";

    string Expr.IVisitor<string>.VisitUnaryExpr(Expr.Unary expr) =>
        Parenthesize(expr.Operator.Lexeme,  expr.Right);

    string Expr.IVisitor<string>.VisitVariableExpr(Expr.Variable expr) =>
        expr.Name.Lexeme;
    
    string Expr.IVisitor<string>.VisitAssignExpr(Expr.Assign expr) =>
        Parenthesize("=", expr.Name.Lexeme, expr);

    private string Parenthesize(string name, params object?[] args)
    {
        var value = string.Join("", args.Select(x => $" {Render(this)}"));
        return $"({name}{value})";
    }

    private string Render(object? value)
    {
        return value switch
        {
            Expr x => x.Accept(this),
            Token x => x.Lexeme,
            _ => value?.ToString() ?? "nil"
        };
    }
}