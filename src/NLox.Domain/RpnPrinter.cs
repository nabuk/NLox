namespace NLox.Domain;

/// <summary>
/// Converts expression tree to Reverse Polish notation (RPN) string representation
/// </summary>
public class RpnPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }
    
    string Expr.IVisitor<string>.VisitBinaryExpr(Expr.Binary expr) =>
        $"{expr.Left.Accept(this)} {expr.Right.Accept(this)} {expr.Operator.Lexeme}";

    string Expr.IVisitor<string>.VisitGroupingExpr(Expr.Grouping expr) =>
        expr.Expression.Accept(this);

    string Expr.IVisitor<string>.VisitLiteralExpr(Expr.Literal expr) =>
        expr.Value?.ToString() ?? "nil";

    string Expr.IVisitor<string>.VisitUnaryExpr(Expr.Unary expr) =>
        $"{expr.Right.Accept(this)} {expr.Operator.Lexeme}";
}