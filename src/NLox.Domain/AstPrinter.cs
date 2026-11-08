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

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var value = string.Join("", exprs.Select(x => $" {x.Accept(this)}"));
        return $"({name}{value})";
    }
}