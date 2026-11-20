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
        Render(expr.Left, expr.Right, expr.Operator.Lexeme);

    string Expr.IVisitor<string>.VisitGroupingExpr(Expr.Grouping expr) =>
        Render(expr.Expression);

    string Expr.IVisitor<string>.VisitLiteralExpr(Expr.Literal expr) =>
        Render(expr.Value);

    string Expr.IVisitor<string>.VisitUnaryExpr(Expr.Unary expr) =>
        Render(expr.Right, expr.Operator.Lexeme);
    
    string Expr.IVisitor<string>.VisitVariableExpr(Expr.Variable expr) =>
        Render(expr.Name);
    
    string Expr.IVisitor<string>.VisitAssignExpr(Expr.Assign expr) =>
        Render(expr.Value, expr.Name, "=");

    string Expr.IVisitor<string>.VisitLogicalExpr(Expr.Logical expr) =>
        Render(expr.Left, expr.Right, expr.Operator.Lexeme);

    string Expr.IVisitor<string>.VisitCallExpr(Expr.Call expr) =>
        Render([.. expr.Arguments.AsEnumerable().Reverse(), expr.Callee, "call"]);

    private string Render(params object?[] args) =>
        string.Join(" ", args.Select(RenderValue));
    
    private string RenderValue(object? value)
    {
        return value switch
        {
            Expr x => x.Accept(this),
            Token x => x.Lexeme,
            _ => value?.ToString() ?? "nil"
        };
    }
}